using BookWorm.Catalog.Infrastructure.GenAi.Ingestion;
using BookWorm.Chassis.RAG;
using BookWorm.Chassis.RAG.Extensions;
using BookWorm.Chassis.RAG.Ingestion;
using BookWorm.Chassis.RAG.Search;

namespace BookWorm.Catalog.Infrastructure.GenAi;

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
