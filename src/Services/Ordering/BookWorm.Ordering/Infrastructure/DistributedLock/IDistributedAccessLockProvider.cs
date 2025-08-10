namespace BookWorm.Ordering.Infrastructure.DistributedLock;

public interface IDistributedAccessLockProvider
{
    IDistributedAccessLock TryAcquire(
        string resourceKey,
        TimeSpan acquireTimeout,
        CancellationToken cancellationToken = default
    );

    Task<IDistributedAccessLock> TryAcquireAsync(
        string resourceKey,
        TimeSpan acquireTimeout,
        CancellationToken cancellationToken = default
    );
}
