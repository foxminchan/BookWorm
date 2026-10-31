using System.Threading.Channels;

namespace BookWorm.Chat.Memory.Extraction;

/// <summary>
///     Hands completed exchanges from the agent to the background extraction service without blocking.
/// </summary>
internal interface IMemoryExtractionQueue
{
    /// <summary>
    ///     Queues an exchange for extraction. Never blocks the caller.
    /// </summary>
    /// <returns><see langword="true" /> if the request was accepted.</returns>
    bool TryEnqueue(MemoryExtractionRequest request);

    /// <summary>
    ///     Reads queued exchanges until <paramref name="cancellationToken" /> is cancelled.
    /// </summary>
    IAsyncEnumerable<MemoryExtractionRequest> ReadAllAsync(CancellationToken cancellationToken);
}

/// <summary>
///     Bounded <see cref="Channel{T}" />-backed queue. When full, the oldest pending exchange is dropped:
///     losing a memory candidate is preferable to slowing down or failing the chat stream.
/// </summary>
internal sealed class MemoryExtractionQueue(UserMemoryOptions options) : IMemoryExtractionQueue
{
    private readonly Channel<MemoryExtractionRequest> _channel =
        Channel.CreateBounded<MemoryExtractionRequest>(
            new BoundedChannelOptions(options.ExtractionQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            }
        );

    public bool TryEnqueue(MemoryExtractionRequest request)
    {
        return _channel.Writer.TryWrite(request);
    }

    public IAsyncEnumerable<MemoryExtractionRequest> ReadAllAsync(
        CancellationToken cancellationToken
    )
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
