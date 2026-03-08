using BookWorm.Catalog.Infrastructure.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Ingestion;
using BookWorm.Chassis.AI.Search;
using BookWorm.Chassis.Caching;
using BookWorm.Constants.Aspire;

namespace BookWorm.Catalog.Infrastructure;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddPersistenceServices()
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
            services.AddQdrantCollection<Guid, TextSnippet>(TextSnippet.CollectionName);

            // Configure Redis Cache
            builder
                .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
                .WithAzureAuthentication();

            builder.AddCaching();

            // Add Blob services
            builder.AddAzureBlobStorage();
        }

        public void AddAI()
        {
            var services = builder.Services;

            builder.AddAIServices().WithAITelemetry();

            services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();

            services.AddHybridSearch();
        }
    }
}
