using BookWorm.Chassis.Caching;
using BookWorm.Chassis.Utilities.Configuration;

namespace BookWorm.Ordering.Infrastructure;

internal static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add database configuration
        builder.AddAzurePostgresDbContext<OrderingDbContext>(
            Components.Database.Ordering,
            _ =>
            {
                services.AddMigration<OrderingDbContext>();

                services.AddRepositories(typeof(IOrderingApiMarker));
            }
        );

        // Configure EventStore
        builder.AddEventStore(storeOptions =>
        {
            storeOptions.ConfigureOrders();
            storeOptions.Projections.DaemonLockId = 44444;
            storeOptions.DisableNpgsqlLogging = true;

            // If we're running in development mode, let Marten just take care
            // of all necessary schema building and patching behind the scenes
            if (builder.Environment.IsDevelopment())
            {
                storeOptions.AutoCreateSchemaObjects = AutoCreate.All;
            }
        });

        // Configure Redis
        builder
            .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
            .WithDistributedCache(options => options.InstanceName = "MainCache");

        services.Configure<CachingOptions>(CachingOptions.ConfigurationSection);

        var cachingOptions = services.BuildServiceProvider().GetRequiredService<CachingOptions>();

        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = cachingOptions.MaximumPayloadBytes;

            options.DefaultEntryOptions = new()
            {
                Expiration = cachingOptions.Expiration,
                LocalCacheExpiration = cachingOptions.Expiration,
            };
        });
    }

    private static void ConfigureOrders(this StoreOptions options)
    {
        // Snapshots
        options.Projections.LiveStreamAggregation<OrderSummary>();
        options.Projections.Add<OrderSummaryViewProjection>(ProjectionLifecycle.Async);
    }
}
