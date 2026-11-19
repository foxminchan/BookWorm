using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Chassis.EventBus.Idempotency;

public interface IIntegrationEventIdempotencyGuard
{
    /// <summary>
    ///     Attempts to acquire a lock for the given event ID and consumer name.
    ///     If the lock is acquired, it will be held for the specified TTL (time-to-live) duration.
    /// </summary>
    /// <param name="eventId">The unique identifier of the integration event.</param>
    /// <param name="consumerName">The name of the consumer attempting to acquire the lock.</param>
    /// <param name="ttl">The optional time-to-live duration for the lock. If not specified, a default TTL will be used.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the lock acquisition.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains a boolean indicating whether the lock was
    ///     successfully acquired (true) or if it was already held by another consumer (false).
    /// </returns>
    Task<bool> TryAcquireAsync(
        Guid eventId,
        string consumerName,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default
    );
}

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

        var key = $"idempotency:{consumerName}:{eventId:N}";
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
}

public static class IntegrationEventIdempotencyExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the opt-in <see cref="IIntegrationEventIdempotencyGuard" /> backed by FusionCache.
        /// </summary>
        public IServiceCollection AddIntegrationEventIdempotency()
        {
            services.TryAddSingleton<
                IIntegrationEventIdempotencyGuard,
                FusionCacheIdempotencyGuard
            >();
            return services;
        }
    }
}
