using BookWorm.Chassis.RAG.Search;
using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.Infrastructure;

internal static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Configure Azure Postgres Database
        builder.AddAzurePostgresDbContext<CatalogDbContext>(
            Components.Database.Catalog,
            app =>
            {
                if (app.Environment.IsDevelopment())
                {
                    services.AddMigration<CatalogDbContext, CatalogDbContextSeed>();
                }
                else
                {
                    services.AddMigration<CatalogDbContext>();
                }

                services.AddRepositories(typeof(ICatalogApiMarker));
            }
        );

        // Configure Qdrant
        builder.AddQdrantClient(Components.VectorDb);
        services.AddQdrantCollection<Guid, TextSnippet>(nameof(Book).ToLowerInvariant());

        // Configure Redis Cache
        builder.AddRedisClient(Components.Redis);

        services.AddHybridCache(options =>
        {
            // Maximum size of cached items
            options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10MB
            options.MaximumKeyLength = 512;

            // Default timeouts
            options.DefaultEntryOptions = new()
            {
                Expiration = TimeSpan.FromMinutes(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(30),
            };
        });

        // Add Blob services
        builder.AddAzureBlobStorage();
    }
}
