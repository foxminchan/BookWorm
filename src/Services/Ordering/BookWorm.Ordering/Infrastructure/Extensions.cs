namespace BookWorm.Ordering.Infrastructure;

public static class Extensions
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
        builder.AddEventStore(options =>
        {
            options.ConfigureOrders();
            options.Projections.DaemonLockId = 44444;
            options.DisableNpgsqlLogging = true;

            // If we're running in development mode, let Marten just take care
            // of all necessary schema building and patching behind the scenes
            if (builder.Environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
        });

        // Configure Redis
        builder.AddRedisClient(Components.Redis);
    }

    private static void ConfigureOrders(this StoreOptions options)
    {
        // Snapshots
        options.Projections.LiveStreamAggregation<OrderSummary>();
        options.Projections.Add<OrderSummaryViewProjection>(ProjectionLifecycle.Async);
    }
}
