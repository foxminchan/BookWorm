using BookWorm.Catalog.Infrastructure.GenAi.Ingestion;
using BookWorm.Chassis.AI;
using BookWorm.Chassis.Ingestion;
using BookWorm.Chassis.Search;

namespace BookWorm.Catalog.Infrastructure.GenAi;

internal static class Extensions
{
    /// <summary>
    /// Configures generative AI services, telemetry, embedding generation, chat completion, ingestion, and search for the application.
    /// </summary>
    /// <param name="builder">The application builder to which GenAI services will be added.</param>
    public static void AddGenAi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddEmbeddingGenerator();

        builder.AddChatCompletion();

        services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();

        services.AddScoped<ISearch, HybridSearch>();
    }
}
