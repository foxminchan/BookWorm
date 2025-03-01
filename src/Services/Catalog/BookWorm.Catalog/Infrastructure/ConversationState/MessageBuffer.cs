using System.Collections.Concurrent;
using StackExchange.Redis;

namespace BookWorm.Catalog.Infrastructure.ConversationState;

public sealed class MessageBuffer : IAsyncDisposable
{
    private const int MaxBufferSize = 20;
    private const int MaxBufferTimeMs = 500;
    private readonly ConcurrentQueue<ClientMessageFragment> _buffer = [];
    private readonly Guid _conversationId;
    private readonly IDatabase _database;
    private readonly TimeSpan _flushInterval = TimeSpan.FromMilliseconds(MaxBufferTimeMs);
    private readonly SemaphoreSlim _flushLock = new(1, 1);
    private readonly Timer _flushTimer;
    private readonly ILogger<RedisConversationState> _logger;
    private readonly ISubscriber _subscriber;

    private int _count;
    private volatile bool _draining;

    public MessageBuffer(
        IDatabase database,
        ISubscriber subscriber,
        ILogger<RedisConversationState> logger,
        Guid conversationId
    )
    {
        _database = database;
        _subscriber = subscriber;
        _logger = logger;
        _conversationId = conversationId;
        _flushTimer = new(_ => TriggerFlush(), null, _flushInterval, _flushInterval);
    }

    // Called once draining is triggered or on disposal.
    // Invariant: By the time DisposeAsync completes, no further flushes are pending.
    public async ValueTask DisposeAsync()
    {
        if (_draining)
        {
            return;
        }

        _draining = true;

        _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);
        await Task.Yield(); // Let any pending timer callback complete

        try
        {
            await TriggerFlushAsync().ConfigureAwait(false);
        }
        finally
        {
            await _flushTimer.DisposeAsync();
            _flushLock.Dispose();
        }
    }

    public async Task AddFragmentAsync(ClientMessageFragment fragment)
    {
        if (_draining)
        {
            throw new InvalidOperationException("Buffer is draining");
        }

        _buffer.Enqueue(fragment);
        // Capture the new count from Increment
        var newCount = Interlocked.Increment(ref _count);
        // Invariant: newCount reflects the number of enqueued fragments.
        if (fragment.IsFinal || newCount >= MaxBufferSize)
        {
            await TriggerFlushAsync().ConfigureAwait(false);
        }
    }

    // Flush is triggered either by the timer or reaching thresholds.
    private async Task TriggerFlushAsync()
    {
        // Attempt to acquire the flush lock without blocking.
        if (!await _flushLock.WaitAsync(0).ConfigureAwait(false))
        {
            return; // Another flush is in progress.
        }

        try
        {
            await FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            // Invariant: _flushLock is always released.
            _flushLock.Release();
        }
    }

    // Timer callback for flush.
    private void TriggerFlush()
    {
        if (_draining)
        {
            return;
        }

        _ = TriggerFlushAsync();
    }

    private async Task FlushAsync()
    {
        var fragmentsToFlush = new List<ClientMessageFragment>();
        try
        {
            // Dequeue until empty and decrement the counter for each item.
            while (_buffer.TryDequeue(out var fragment))
            {
                fragmentsToFlush.Add(fragment);
                Interlocked.Decrement(ref _count);
            }

            if (fragmentsToFlush.Count > 0)
            {
                // Log before initiating IO operations.
                _logger.LogInformation(
                    "Flushing {Count} fragments for conversation {ConversationId} {MessageId}",
                    fragmentsToFlush.Count,
                    _conversationId,
                    fragmentsToFlush[0].Id
                );

                var key = RedisConversationState.GetBacklogKey(_conversationId);
                var channel = RedisConversationState.GetRedisChannelName(_conversationId);
                var coalescedFragment = CoalesceFragments(fragmentsToFlush);
                var serialized = JsonSerializer.Serialize(coalescedFragment);

                // Use WhenAll to send to Redis in parallel.
                await Task.WhenAll(
                        _database.ListRightPushAsync(key, serialized),
                        _subscriber.PublishAsync(channel, serialized)
                    )
                    .ConfigureAwait(false);

                // Log after successful IO.
                _logger.LogInformation(
                    "Successfully flushed fragments for conversation {ConversationId} {MessageId}",
                    _conversationId,
                    fragmentsToFlush[0].Id
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error flushing fragments for conversation {ConversationId}. Re-enqueueing fragments.",
                _conversationId
            );

            // On failure, re-enqueue fragments and re-adjust the counter.
            foreach (var fragment in fragmentsToFlush)
            {
                _buffer.Enqueue(fragment);
                Interlocked.Increment(ref _count);
            }

            throw;
        }
    }

    public static ClientMessageFragment CoalesceFragments(List<ClientMessageFragment> fragments)
    {
        var lastFragment = fragments[^1];
        var count = fragments.Count;
        var totalLength = 0;
        for (var i = 0; i < count; i++)
        {
            totalLength += fragments[i].Text.Length;
        }

        var combined = string.Create(
            totalLength,
            fragments,
            (span, frags) =>
            {
                var pos = 0;
                foreach (var t in frags)
                {
                    ReadOnlySpan<char> text = t.Text;
                    text.CopyTo(span[pos..]);
                    pos += text.Length;
                }
            }
        );
        return lastFragment with { Text = combined };
    }
}
