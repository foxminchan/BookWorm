namespace BookWorm.Ordering.Infrastructure.DistributedLock;

public interface IDistributedAccessLock : IDisposable, IAsyncDisposable
{
    bool IsAcquired { get; }

    Exception? Exception { get; }
}
