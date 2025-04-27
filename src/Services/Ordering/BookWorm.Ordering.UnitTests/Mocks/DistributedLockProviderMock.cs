using Medallion.Threading;

namespace BookWorm.Ordering.UnitTests.Mocks;

public sealed class DistributedLockProviderMock(IDistributedSynchronizationHandle handle)
    : IDistributedLockProvider
{
    public IDistributedLock CreateLock(string name)
    {
        return new TestDistributedLock(handle, name);
    }

    private sealed class TestDistributedLock(IDistributedSynchronizationHandle handle, string name)
        : IDistributedLock
    {
        public string Name { get; } = name;

        public IDistributedSynchronizationHandle Acquire(
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default
        )
        {
            return handle;
        }

        public ValueTask<IDistributedSynchronizationHandle> AcquireAsync(
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(handle);
        }

        public IDistributedSynchronizationHandle TryAcquire(
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default
        )
        {
            return handle;
        }

        public ValueTask<IDistributedSynchronizationHandle?> TryAcquireAsync(
            TimeSpan timeout = default,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(handle)!;
        }
    }
}
