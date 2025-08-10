namespace BookWorm.Ordering.Infrastructure.DistributedLock;

public sealed class DisposableDistributedAccessLock<TDistributedLock> : IDistributedAccessLock
    where TDistributedLock : IDisposable, IAsyncDisposable
{
    public DisposableDistributedAccessLock(bool isAcquired, TDistributedLock? distributedLock)
    {
        IsAcquired = isAcquired;
        DistributedLock = distributedLock;
        Exception = null;
    }

    public DisposableDistributedAccessLock(Exception exception)
    {
        IsAcquired = false;
        DistributedLock = default;
        Exception = exception;
    }

    private TDistributedLock? DistributedLock { get; }

    public bool IsAcquired { get; }

    public Exception? Exception { get; set; }

    public void Dispose()
    {
        DistributedLock?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return DistributedLock is not null ? DistributedLock.DisposeAsync() : default;
    }
}
