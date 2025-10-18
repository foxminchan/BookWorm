﻿using BookWorm.Catalog.Infrastructure.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Ingestion;
using BookWorm.Chassis.AI.Search;
using BookWorm.Chassis.Caching;
using BookWorm.Constants.Aspire;
using BookWorm.ServiceDefaults.Configuration;

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
