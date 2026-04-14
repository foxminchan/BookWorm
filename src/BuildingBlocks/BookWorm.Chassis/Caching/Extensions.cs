using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Caching;

public static class CachingExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers hybrid caching services with the dependency injection container,
        ///     configuring options from <see cref="CachingOptions" /> and an optional delegate.
        /// </summary>
        /// <param name="configure">
        ///     An optional delegate to further configure <see cref="HybridCacheOptions" />
        ///     after the defaults derived from <see cref="CachingOptions" /> have been applied.
        /// </param>
        public void AddCaching(Action<HybridCacheOptions>? configure = null)
        {
            var services = builder.Services;

            builder.Configure<CachingOptions>(CachingOptions.ConfigurationSection);

            var cachingOptions = services
                .BuildServiceProvider()
                .GetRequiredService<CachingOptions>();

            services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = cachingOptions.MaximumPayloadBytes;

                options.DefaultEntryOptions = new()
                {
                    Expiration = cachingOptions.Expiration,
                    LocalCacheExpiration = cachingOptions.Expiration,
                };

                configure?.Invoke(options);
            });

            services.AddSingleton<IHybridCache, HybridCacheService>();
        }
    }
}
