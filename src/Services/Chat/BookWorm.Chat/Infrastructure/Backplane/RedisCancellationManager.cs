using BookWorm.Chat.Infrastructure.Backplane.Contracts;

namespace BookWorm.Chat.Infrastructure.Backplane;

public sealed class RedisCancellationManager : ICancellationManager, IDisposable
{
    private readonly RedisChannel _channelName = RedisChannel.Literal(
        $"{nameof(Chat).ToLowerInvariant()}-{nameof(CancellationToken).ToLowerInvariant()}"
    );

    private readonly ConcurrentDictionary<
        Guid,
        ConcurrentDictionary<Guid, byte>
    > _conversationToMessages = [];
    private readonly ConcurrentDictionary<Guid, Guid> _messageToConversation = [];

    private readonly ILogger<RedisCancellationManager> _logger;
    private readonly ISubscriber _subscriber;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _tokens = [];

    public RedisCancellationManager(
        IConnectionMultiplexer redis,
        ILogger<RedisCancellationManager> logger
    )
    {
        _subscriber = redis.GetSubscriber();
        _logger = logger;
        _subscriber.Subscribe(_channelName, OnCancellationMessage);
        _logger.LogInformation("Subscribed to cancellation channel {Channel}", _channelName);
    }

    public CancellationToken GetCancellationToken(Guid conversationId, Guid messageId)
    {
        var cts = new CancellationTokenSource();
        _tokens[messageId] = cts;

        var messageSet = _conversationToMessages.GetOrAdd(
            conversationId,
            _ => new ConcurrentDictionary<Guid, byte>()
        );
        messageSet.TryAdd(messageId, 0);

        _messageToConversation[messageId] = conversationId;

        _logger.LogDebug(
            "Created cancellation token for conversation {ConversationId}, message {MessageId}",
            conversationId,
            messageId
        );

        return cts.Token;
    }

    public async Task CancelAsync(Guid conversationId)
    {
        _logger.LogInformation(
            "Publishing cancellation message for conversation {ConversationId}",
            conversationId
        );

        if (!_conversationToMessages.TryGetValue(conversationId, out var messageIds))
        {
            _logger.LogWarning(
                "No active messages found for conversation {ConversationId}",
                conversationId
            );

            return;
        }

        var messagesToCancel = messageIds.Keys.ToList();

        foreach (var messageId in messagesToCancel)
        {
            await _subscriber.PublishAsync(_channelName, messageId.ToString());

            _logger.LogDebug(
                "Published cancellation for message {MessageId} in conversation {ConversationId}",
                messageId,
                conversationId
            );
        }
    }

    public void Dispose()
    {
        try
        {
            _subscriber.Unsubscribe(_channelName);

            _logger.LogInformation(
                "Unsubscribed from cancellation channel {Channel}",
                _channelName
            );

            foreach (var kvp in _tokens)
            {
                kvp.Value.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RedisCancellationManager disposal");
        }
    }

    private void OnCancellationMessage(RedisChannel channel, RedisValue message)
    {
        if (!Guid.TryParse(message.ToString(), out var messageId))
        {
            _logger.LogWarning("Received invalid cancellation message: {Message}", message);
            return;
        }

        _logger.LogInformation("Received cancellation message for message {MessageId}", messageId);

        if (!_tokens.TryRemove(messageId, out var cts))
        {
            _logger.LogDebug(
                "Cancellation token not found for message {MessageId} (may have already completed)",
                messageId
            );

            return;
        }

        cts.Cancel();
        cts.Dispose();

        _logger.LogInformation("Cancelled token for message {MessageId}", messageId);

        CleanupConversationMapping(messageId);
    }

    private void CleanupConversationMapping(Guid messageId)
    {
        if (!_messageToConversation.TryRemove(messageId, out var conversationId))
        {
            _logger.LogDebug(
                "Message {MessageId} not found in reverse mapping (may have already been cleaned up)",
                messageId
            );
            return;
        }

        if (!_conversationToMessages.TryGetValue(conversationId, out var messageIds))
        {
            return;
        }

        messageIds.TryRemove(messageId, out _);

        if (messageIds.IsEmpty)
        {
            _conversationToMessages.TryRemove(conversationId, out _);

            _logger.LogDebug("Removed empty conversation {ConversationId} mapping", conversationId);
        }
    }
}
