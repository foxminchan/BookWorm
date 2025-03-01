﻿using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using StackExchange.Redis;

namespace BookWorm.Catalog.Infrastructure.ConversationState;

public sealed class RedisConversationState : IConversationState, IDisposable
{
    private static readonly ConcurrentDictionary<
        Guid,
        List<Action<ClientMessageFragment>>
    > GlobalSubscribers = new();

    private readonly IDatabase _database;
    private readonly ILogger<RedisConversationState> _logger;

    private readonly ConcurrentDictionary<Guid, MessageBuffer> _messageBuffers = [];

    private readonly RedisChannel _patternChannel;
    private readonly ISubscriber _subscriber;

    public RedisConversationState(
        IConnectionMultiplexer redis,
        ILogger<RedisConversationState> logger
    )
    {
        _database = redis.GetDatabase();
        _subscriber = redis.GetSubscriber();
        _logger = logger;
        _patternChannel = RedisChannel.Pattern("conversation:*:channel");
        _subscriber.Subscribe(_patternChannel, OnRedisMessage);
        _logger.LogInformation("Subscribed to pattern {Pattern}", _patternChannel);
    }

    public async Task CompleteAsync(Guid conversationId, Guid messageId)
    {
        if (_messageBuffers.TryRemove(messageId, out var buffer))
        {
            await buffer.DisposeAsync();
        }

        await _database.KeyExpireAsync(GetBacklogKey(conversationId), TimeSpan.FromMinutes(5));
    }

    public async Task PublishFragmentAsync(Guid conversationId, ClientMessageFragment fragment)
    {
        _logger.LogDebug(
            "Publishing fragment {FragmentId} for conversation {ConversationId} to Redis",
            fragment.Id,
            conversationId
        );

        var buffer = _messageBuffers.GetOrAdd(
            fragment.Id,
            _ => new(_database, _subscriber, _logger, conversationId)
        );
        await buffer.AddFragmentAsync(fragment);
    }

    public async IAsyncEnumerable<ClientMessageFragment> Subscribe(
        Guid conversationId,
        Guid? lastMessageId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "New Redis subscription for conversation {ConversationId} with lastMessageId {LastMessageId}",
            conversationId,
            lastMessageId
        );
        var channel = Channel.CreateUnbounded<ClientMessageFragment>();

        AddLocalSubscriber(conversationId, LocalCallback);

        var key = GetBacklogKey(conversationId);
        var values = await _database.ListRangeAsync(key);
        foreach (var t in values)
        {
            var fragment = JsonSerializer.Deserialize<ClientMessageFragment>((byte[])t!);
            if (fragment is not null && (lastMessageId is null || fragment.Id > lastMessageId))
            {
                yield return fragment;
            }
        }

        try
        {
            await using var reg = cancellationToken.Register(() => channel.Writer.TryComplete());
            await foreach (var fragment in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return fragment;
            }
        }
        finally
        {
            RemoveLocalSubscriber(conversationId, LocalCallback);
            _logger.LogInformation(
                "Redis subscription for conversation {ConversationId} ended",
                conversationId
            );
        }

        yield break;

        // Register a local callback BEFORE retrieving the backlog.
        void LocalCallback(ClientMessageFragment fragment)
        {
            if (lastMessageId != null && fragment.Id <= lastMessageId)
            {
                return;
            }

            _logger.LogDebug(
                "Fanning out fragment {FragmentId} for conversation {ConversationId}",
                fragment.Id,
                conversationId
            );

            channel.Writer.TryWrite(fragment);
        }
    }

    public async Task<IList<ClientMessage>> GetUnpublishedMessagesAsync(Guid conversationId)
    {
        var key = GetBacklogKey(conversationId);
        var values = await _database.ListRangeAsync(key);
        var fragments = new List<ClientMessageFragment>(values.Length);
        fragments.AddRange(
            values
                .Select(t => JsonSerializer.Deserialize<ClientMessageFragment>((byte[])t!))
                .OfType<ClientMessageFragment>()
        );

        return fragments
            .GroupBy(f => f.Id)
            .Select(g => MessageBuffer.CoalesceFragments([.. g.Skip(1)]))
            .Select(coalescedFragment => new ClientMessage(
                coalescedFragment.Id,
                coalescedFragment.Sender,
                coalescedFragment.Text
            ))
            .ToList();
    }

    public void Dispose()
    {
        try
        {
            _subscriber.Unsubscribe(_patternChannel);
            _logger.LogInformation("Unsubscribed from pattern {Pattern}", _patternChannel);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during unsubscription for pattern {Pattern}",
                _patternChannel
            );
        }
    }

    public static RedisKey GetBacklogKey(Guid conversationId)
    {
        return $"conversation:{conversationId}:backlog";
    }

    public static RedisChannel GetRedisChannelName(Guid conversationId)
    {
        return RedisChannel.Literal($"conversation:{conversationId}:channel");
    }

    private void OnRedisMessage(RedisChannel channel, RedisValue value)
    {
        var channelStr = channel.ToString();
        var parts = channelStr.Split(':');
        if (parts.Length < 3 || !Guid.TryParse(parts[1], out var conversationId))
        {
            return;
        }

        ClientMessageFragment? fragment;

        try
        {
            fragment = JsonSerializer.Deserialize<ClientMessageFragment>((byte[])value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing message from channel {Channel}", channel);
            return;
        }

        if (fragment is null)
        {
            return;
        }

        _logger.LogDebug(
            "Redis message received for conversation {ConversationId} with fragment {FragmentId}",
            conversationId,
            fragment.Id
        );

        if (!GlobalSubscribers.TryGetValue(conversationId, out var subscribers))
        {
            return;
        }

        lock (subscribers)
        {
            foreach (var sub in subscribers)
            {
                sub(fragment);
            }
        }
    }

    private static void RemoveLocalSubscriber(
        Guid conversationId,
        Action<ClientMessageFragment> callback
    )
    {
        if (!GlobalSubscribers.TryGetValue(conversationId, out var list))
        {
            return;
        }

        lock (list)
        {
            list.Remove(callback);
            if (list.Count == 0)
            {
                GlobalSubscribers.TryRemove(conversationId, out _);
            }
        }
    }

    private static void AddLocalSubscriber(
        Guid conversationId,
        Action<ClientMessageFragment> callback
    )
    {
        var list = GlobalSubscribers.GetOrAdd(conversationId, _ => []);
        lock (list)
        {
            list.Add(callback);
        }
    }
}
