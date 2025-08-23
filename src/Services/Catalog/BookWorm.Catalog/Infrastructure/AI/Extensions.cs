using BookWorm.Catalog.Infrastructure.AI.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Ingestion;
using BookWorm.Chassis.AI.Search;

namespace BookWorm.Catalog.Infrastructure.AI;

internal static class Extensions
{
    public static void AddGenAi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();

        builder.AddEmbeddingGenerator();

        services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();

        services.AddScoped<ISearch, HybridSearch>();
    }
}
