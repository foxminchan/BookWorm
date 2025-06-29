using BookWorm.Catalog.Infrastructure.GenAi.Ingestion;
using BookWorm.Chassis.AI;
using BookWorm.Chassis.Ingestion;
using BookWorm.Chassis.Search;

namespace BookWorm.Catalog.Infrastructure.GenAi;

public static class Extensions
{
    public static void AddGenAi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddSkTelemetry();

        builder.AddEmbeddingGenerator();

        builder.AddChatCompletion();

        services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();

        services.AddScoped<ISearch, HybridSearch>();
    }
}
