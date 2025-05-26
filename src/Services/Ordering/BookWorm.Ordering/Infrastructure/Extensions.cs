using BookWorm.Constants.Aspire;

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
            options.Projections.LiveStreamAggregation<OrderSummary>();
            options.Projections.Add<Projection>(ProjectionLifecycle.Async);
        });

        // Configure Redis
        builder.AddRedisClient(Components.Redis);
    }
}
