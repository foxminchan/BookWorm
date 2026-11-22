using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Chassis.EventBus.Idempotency;

internal sealed class FusionCacheIdempotencyGuard(
    IFusionCache cache,
    IConnectionMultiplexer? redis = null
) : IIntegrationEventIdempotencyGuard
{
    private static readonly TimeSpan _defaultTtl = TimeSpan.FromHours(24);
    private readonly SemaphoreSlim _inMemoryLock = new(1, 1);

    public async Task<bool> TryAcquireAsync(
        Guid eventId,
        string consumerName,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);
        cancellationToken.ThrowIfCancellationRequested();

        var key = FormatKey(consumerName, eventId);
        var effectiveTtl = ttl ?? _defaultTtl;

        if (redis is not null)
        {
            var acquired = await redis
                .GetDatabase()
                .StringSetAsync(key, "1", effectiveTtl, When.NotExists);

            if (!acquired)
            {
                return false;
            }

            await cache.SetAsync(
                key,
                true,
                new FusionCacheEntryOptions(effectiveTtl) { SkipDistributedCacheWrite = true },
                cancellationToken
            );

            return true;
        }

        await _inMemoryLock.WaitAsync(cancellationToken);
        try
        {
            var existing = await cache.TryGetAsync<bool>(key, token: cancellationToken);

            if (existing.HasValue)
            {
                return false;
            }

            await cache.SetAsync(
                key,
                true,
                new FusionCacheEntryOptions(effectiveTtl),
                cancellationToken
            );

            return true;
        }
        finally
        {
            _inMemoryLock.Release();
        }
    }

    public async Task ReleaseAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerName);
        var key = FormatKey(consumerName, eventId);

        if (redis is not null)
        {
            await redis.GetDatabase().KeyDeleteAsync(key);
        }

        await cache.RemoveAsync(key, token: cancellationToken);
    }

    private static string FormatKey(string consumerName, Guid eventId)
    {
        return $"idempotency:{consumerName}:{eventId:N}";
    }
}
