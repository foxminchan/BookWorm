using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed.Redis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace BookWorm.Chassis.Caching;

public static class CachingExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers FusionCache (L1 + L2 + Backplane) and <see cref="IDistributedCache" />
        ///     with the dependency injection container, reusing the <see cref="IConnectionMultiplexer" />
        ///     registered by Aspire's Redis client (or falling back to an in-memory distributed cache).
        /// </summary>
        /// <param name="configure">
        ///     An optional delegate to further configure the <see cref="IFusionCacheBuilder" />
        ///     after the defaults derived from <see cref="CachingOptions" /> have been applied.
        /// </param>
        public IHostApplicationBuilder AddCaching(Action<IFusionCacheBuilder>? configure = null)
        {
            var services = builder.Services;
            var servicePrefix = builder.Environment.ApplicationName;

            builder.Configure<CachingOptions>(CachingOptions.ConfigurationSection);

            var fusionBuilder = services
                .AddFusionCache()
                .WithSerializer(new FusionCacheSystemTextJsonSerializer());

            services
                .AddOptions<FusionCacheOptions>()
                .Configure<CachingOptions>(
                    (options, cachingOptions) =>
                    {
                        if (!string.IsNullOrWhiteSpace(servicePrefix))
                        {
                            if (string.IsNullOrEmpty(options.CacheKeyPrefix))
                            {
                                options.CacheKeyPrefix = $"{servicePrefix}:";
                            }

                            if (string.IsNullOrEmpty(options.BackplaneChannelPrefix))
                            {
                                options.BackplaneChannelPrefix = servicePrefix;
                            }
                        }

                        options.DefaultEntryOptions.Duration = cachingOptions.Expiration;
                        options.DefaultEntryOptions.DistributedCacheDuration =
                            cachingOptions.Expiration;
                        options.DefaultEntryOptions.IsFailSafeEnabled = true;
                        options.DefaultEntryOptions.FailSafeMaxDuration = TimeSpan.FromHours(2);
                        options.DefaultEntryOptions.FailSafeThrottleDuration = TimeSpan.FromSeconds(
                            30
                        );
                        options.DefaultEntryOptions.FactorySoftTimeout = TimeSpan.FromMilliseconds(
                            500
                        );
                        options.DefaultEntryOptions.DistributedCacheSoftTimeout =
                            TimeSpan.FromMilliseconds(200);
                        options.DefaultEntryOptions.DistributedCacheHardTimeout =
                            TimeSpan.FromSeconds(2);
                        options.DefaultEntryOptions.AllowBackgroundDistributedCacheOperations =
                            true;
                        options.DefaultEntryOptions.JitterMaxDuration = TimeSpan.FromSeconds(5);
                    }
                );

            if (services.Any(d => d.ServiceType == typeof(IConnectionMultiplexer)))
            {
                services.TryAddSingleton<IDistributedCache>(sp => new RedisCache(
                    new RedisCacheOptions
                    {
                        ConnectionMultiplexerFactory = () =>
                            Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>()),
                    }
                ));

                fusionBuilder.WithRegisteredDistributedCache();

                fusionBuilder.WithBackplane(sp => new RedisBackplane(
                    new RedisBackplaneOptions
                    {
                        ConnectionMultiplexerFactory = () =>
                            Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>()),
                    }
                ));
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services
                .AddOpenTelemetry()
                .WithMetrics(metrics => metrics.AddFusionCacheInstrumentation())
                .WithTracing(tracing => tracing.AddFusionCacheInstrumentation());

            configure?.Invoke(fusionBuilder);

            return builder;
        }

        /// <summary>
        ///     Registers <see cref="IFusionCacheDistributedLocker" /> backed by Redis,
        ///     reusing the <see cref="IConnectionMultiplexer" /> from dependency injection.
        /// </summary>
        public void AddDistributedLock()
        {
            builder.Services.AddSingleton<IFusionCacheDistributedLocker>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                var options = Options.Create(
                    new RedisDistributedLockerOptions
                    {
                        ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer),
                    }
                );
                var logger = sp.GetRequiredService<ILogger<RedisDistributedLocker>>();
                return new RedisDistributedLocker(options, logger);
            });
        }
    }
}
