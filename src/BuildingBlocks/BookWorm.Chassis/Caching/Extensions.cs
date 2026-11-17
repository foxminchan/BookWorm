using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace BookWorm.Chassis.Caching;

public static class CachingExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers FusionCache (L1 + L2 + Backplane) with the dependency injection container,
        ///     reusing the <see cref="IConnectionMultiplexer" /> registered by Aspire's Redis client
        ///     for both the distributed cache and the Redis backplane.
        /// </summary>
        /// <param name="configure">
        ///     An optional delegate to further configure the <see cref="IFusionCacheBuilder" />
        ///     after the defaults derived from <see cref="CachingOptions" /> have been applied.
        /// </param>
        public void AddCaching(Action<IFusionCacheBuilder>? configure = null)
        {
            var services = builder.Services;

            builder.Configure<CachingOptions>(CachingOptions.ConfigurationSection);

            var fusionBuilder = services
                .AddFusionCache()
                .WithSerializer(new FusionCacheSystemTextJsonSerializer());

            services
                .AddOptions<FusionCacheOptions>()
                .Configure<CachingOptions>(
                    (options, cachingOptions) =>
                    {
                        options.DefaultEntryOptions.Duration = cachingOptions.Expiration;
                        options.DefaultEntryOptions.DistributedCacheDuration =
                            cachingOptions.Expiration;
                    }
                );

            if (services.Any(d => d.ServiceType == typeof(IConnectionMultiplexer)))
            {
                fusionBuilder.WithDistributedCache(sp => new RedisCache(
                    new RedisCacheOptions
                    {
                        ConnectionMultiplexerFactory = () =>
                            Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>()),
                    }
                ));

                fusionBuilder.WithBackplane(sp => new RedisBackplane(
                    new RedisBackplaneOptions
                    {
                        ConnectionMultiplexerFactory = () =>
                            Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>()),
                    }
                ));
            }

            services
                .AddOpenTelemetry()
                .WithMetrics(metrics => metrics.AddFusionCacheInstrumentation())
                .WithTracing(tracing => tracing.AddFusionCacheInstrumentation());

            configure?.Invoke(fusionBuilder);
        }
    }
}
