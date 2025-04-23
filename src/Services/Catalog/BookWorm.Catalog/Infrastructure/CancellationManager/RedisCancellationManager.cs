using System.Collections.Concurrent;
using OllamaSharp;
using StackExchange.Redis;

namespace BookWorm.Catalog.Infrastructure.CancellationManager;

public sealed class RedisCancellationManager : ICancellationManager, IDisposable
{
    private readonly RedisChannel _channelName = RedisChannel.Literal(
        $"{nameof(Chat).ToLower()}-{nameof(CancellationToken).ToLower()}"
    );
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

    public CancellationToken GetCancellationToken(Guid id)
    {
        var cts = new CancellationTokenSource();
        _tokens[id] = cts;

        _logger.LogDebug("Created cancellation token for reply {ReplyId}", id);

        return cts.Token;
    }

    public async Task CancelAsync(Guid id)
    {
        _logger.LogDebug("Publishing cancellation message for reply {ReplyId}", id);

        await _subscriber.PublishAsync(_channelName, id.ToString());
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
        if (Guid.TryParse(message, out var replyId))
        {
            _logger.LogInformation("Received cancellation message for reply {ReplyId}", replyId);

            if (!_tokens.TryRemove(replyId, out var cts))
            {
                return;
            }

            cts.Cancel();
            _logger.LogInformation("Cancelled token for reply {ReplyId}", replyId);
        }
        else
        {
            _logger.LogWarning("Received invalid cancellation message: {Message}", message);
        }
    }
}
