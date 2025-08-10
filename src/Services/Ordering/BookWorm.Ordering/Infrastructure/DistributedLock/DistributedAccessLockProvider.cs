namespace BookWorm.Ordering.Infrastructure.DistributedLock;

public sealed class DistributedAccessLockProvider(IDistributedLockProvider synchronizationProvider)
    : IDistributedAccessLockProvider
{
    public IDistributedAccessLock TryAcquire(
        string resourceKey,
        TimeSpan acquireTimeout,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var distributedSynchronizationHandle = synchronizationProvider.AcquireLock(
                resourceKey,
                acquireTimeout,
                cancellationToken
            );
            return new DisposableDistributedAccessLock<IDistributedSynchronizationHandle>(
                true,
                distributedSynchronizationHandle
            );
        }
        catch (Exception ex)
        {
            return new DisposableDistributedAccessLock<IDistributedSynchronizationHandle>(ex);
        }
    }

    public async Task<IDistributedAccessLock> TryAcquireAsync(
        string resourceKey,
        TimeSpan acquireTimeout,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var distributedSynchronizationHandle = await synchronizationProvider.AcquireLockAsync(
                resourceKey,
                acquireTimeout,
                cancellationToken
            );
            return new DisposableDistributedAccessLock<IDistributedSynchronizationHandle>(
                true,
                distributedSynchronizationHandle
            );
        }
        catch (Exception ex)
        {
            return new DisposableDistributedAccessLock<IDistributedSynchronizationHandle>(ex);
        }
    }
}
