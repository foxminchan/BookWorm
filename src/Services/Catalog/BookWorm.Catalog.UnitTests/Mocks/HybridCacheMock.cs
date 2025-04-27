using Microsoft.Extensions.Caching.Hybrid;

namespace BookWorm.Catalog.UnitTests.Mocks;

public sealed class HybridCacheMock : HybridCache
{
    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    )
    {
        return factory(state, cancellationToken);
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return default;
    }

    public override ValueTask RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default
    )
    {
        return default;
    }

    public override ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    )
    {
        return default;
    }
}
