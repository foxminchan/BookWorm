using System.Threading.Channels;
using BookWorm.Chat.Features;
using BookWorm.Chat.Infrastructure.Backplane.Contracts;
using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Chat.Infrastructure.Backplane;

public sealed partial class RedisConversationState : IConversationState, IDisposable
{
    private static readonly ConcurrentDictionary<
        Guid,
        List<Action<ClientMessageFragment>>
    > _globalSubscribers = new();

    private readonly IDatabase _database;
    private readonly GlobalLogBuffer _logBuffer;
    private readonly ILogger<RedisConversationState> _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly ConcurrentDictionary<Guid, MessageBuffer> _messageBuffers = [];

    private readonly RedisChannel _patternChannel;
    private readonly ISubscriber _subscriber;

    public RedisConversationState(
        IConnectionMultiplexer redis,
        ILogger<RedisConversationState> logger,
        ILoggerFactory loggerFactory,
        GlobalLogBuffer logBuffer
    )
    {
        _database = redis.GetDatabase();
        _subscriber = redis.GetSubscriber();
        _logger = logger;
        _loggerFactory = loggerFactory;
        _logBuffer = logBuffer;
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

    /// <summary>
    /// Publishes a message fragment to the Redis backplane for the specified conversation.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="fragment">The message fragment to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishFragmentAsync(Guid conversationId, ClientMessageFragment fragment)
    {
        _logger.LogDebug(
            "Publishing fragment {FragmentId} for conversation {ConversationId} to Redis",
            fragment.Id,
            conversationId
        );

        var buffer = _messageBuffers.GetOrAdd(
            fragment.Id,
            _ =>
                new(
                    _database,
                    _subscriber,
                    _loggerFactory.CreateLogger<MessageBuffer>(),
                    conversationId,
                    _logBuffer
                )
        );

        await buffer.AddFragmentAsync(fragment);
    }

    /// <summary>
    /// Subscribes to real-time and backlog message fragments for a conversation, yielding fragments newer than the specified last message ID.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation to subscribe to.</param>
    /// <param name="lastMessageId">The ID of the last message fragment received by the subscriber, or null to receive all fragments.</param>
    /// <param name="cancellationToken">Token to cancel the subscription and stop yielding fragments.</param>
    /// <returns>An asynchronous stream of <see cref="ClientMessageFragment"/> objects representing new and backlog message fragments for the conversation.</returns>
    public async IAsyncEnumerable<ClientMessageFragment> SubscribeAsync(
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
            if (lastMessageId is not null && fragment.Id <= lastMessageId)
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

        return
        [
            .. fragments
                .GroupBy(f => f.Id)
                .Select(g => MessageBuffer.CoalesceFragments([.. g.Skip(1)]))
                .Select(coalescedFragment => new ClientMessage(
                    coalescedFragment.Id,
                    coalescedFragment.Sender,
                    coalescedFragment.Text
                )),
        ];
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

            _logBuffer.Flush();
        }
    }

    private static RedisKey GetBacklogKey(Guid conversationId)
    {
        return $"conversation:{conversationId}:backlog";
    }

    private static RedisChannel GetRedisChannelName(Guid conversationId)
    {
        return RedisChannel.Literal($"conversation:{conversationId}:channel");
    }

    private void OnRedisMessage(RedisChannel channel, RedisValue value)
    {
        var parts = channel.ToString().Split(':');

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
            _logBuffer.Flush();
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

        if (!_globalSubscribers.TryGetValue(conversationId, out var subscribers))
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
        if (!_globalSubscribers.TryGetValue(conversationId, out var list))
        {
            return;
        }

        lock (list)
        {
            list.Remove(callback);

            if (list.Count == 0)
            {
                _globalSubscribers.TryRemove(conversationId, out _);
            }
        }
    }

    private static void AddLocalSubscriber(
        Guid conversationId,
        Action<ClientMessageFragment> callback
    )
    {
        var list = _globalSubscribers.GetOrAdd(conversationId, _ => []);
        lock (list)
        {
            list.Add(callback);
        }
    }
}
