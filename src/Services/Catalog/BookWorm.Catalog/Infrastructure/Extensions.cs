using BookWorm.Catalog.Infrastructure.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Ingestion;
using BookWorm.Chassis.AI.Search;
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
        builder
            .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
            .WithDistributedCache(options => options.InstanceName = "MainCache");

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

    public static void AddAI(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddChatClient();

        builder.AddEmbeddingGenerator();

        builder.AddAgentsTelemetry();

        services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();

        services.AddScoped<ISearch, HybridSearch>();
    }
}
