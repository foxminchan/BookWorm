using Microsoft.Extensions.Caching.Hybrid;

namespace BookWorm.Chassis.Caching;

internal sealed class HybridCacheService(HybridCache cache) : IHybridCache
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetOrCreateAsync(
            key,
            factory,
            tags: tags,
            cancellationToken: cancellationToken
        );
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(key, cancellationToken);
    }

    public ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return cache.RemoveByTagAsync(tag, cancellationToken);
    }

    public ValueTask SetAsync<T>(
        string key,
        T value,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(key, value, tags: tags, cancellationToken: cancellationToken);
    }
}
