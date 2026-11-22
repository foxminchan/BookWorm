using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookWorm.Chassis.EventBus.Idempotency;

public static class IntegrationEventIdempotencyExtensions
{
    extension(IIntegrationEventIdempotencyGuard? idempotencyGuard)
    {
        /// <summary>
        ///     Executes <paramref name="action" /> only if the idempotency guard is acquired (or absent),
        ///     and releases the lock if <paramref name="action" /> throws an exception so broker retries can proceed.
        /// </summary>
        public async Task ExecuteAsync(
            Guid eventId,
            string consumerName,
            Func<CancellationToken, Task> action,
            TimeSpan? ttl = null,
            CancellationToken cancellationToken = default
        )
        {
            if (
                idempotencyGuard is not null
                && !await idempotencyGuard.TryAcquireAsync(
                    eventId,
                    consumerName,
                    ttl,
                    cancellationToken
                )
            )
            {
                return;
            }

            try
            {
                await action(cancellationToken);
            }
            catch
            {
                if (idempotencyGuard is not null)
                {
                    await idempotencyGuard.ReleaseAsync(
                        eventId,
                        consumerName,
                        CancellationToken.None
                    );
                }

                throw;
            }
        }

        /// <summary>
        ///     Executes <paramref name="action" /> only if the idempotency guard is acquired (or absent),
        ///     returning <c>default</c> when already processed, and releases the lock if <paramref name="action" /> throws.
        /// </summary>
        public async Task<TResult?> ExecuteAsync<TResult>(
            Guid eventId,
            string consumerName,
            Func<CancellationToken, Task<TResult?>> action,
            TimeSpan? ttl = null,
            CancellationToken cancellationToken = default
        )
        {
            if (
                idempotencyGuard is not null
                && !await idempotencyGuard.TryAcquireAsync(
                    eventId,
                    consumerName,
                    ttl,
                    cancellationToken
                )
            )
            {
                return default;
            }

            try
            {
                return await action(cancellationToken);
            }
            catch
            {
                if (idempotencyGuard is not null)
                {
                    await idempotencyGuard.ReleaseAsync(
                        eventId,
                        consumerName,
                        CancellationToken.None
                    );
                }

                throw;
            }
        }
    }

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
