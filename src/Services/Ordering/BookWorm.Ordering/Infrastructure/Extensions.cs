using BookWorm.Chassis.Caching;

namespace BookWorm.Ordering.Infrastructure;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddPersistenceServices()
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
                .WithAzureAuthentication();

            builder.AddCaching();
        }
    }

    extension(StoreOptions options)
    {
        private void ConfigureOrders()
        {
            // Snapshots
            options.Projections.LiveStreamAggregation<OrderSummary>();
            options.Projections.Add<OrderSummaryViewProjection>(ProjectionLifecycle.Async);
        }
    }
}
