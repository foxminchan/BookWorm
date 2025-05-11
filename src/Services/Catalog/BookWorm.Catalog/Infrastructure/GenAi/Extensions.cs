using BookWorm.Catalog.Infrastructure.GenAi.Ingestion;
using BookWorm.Chassis.Ingestion;
using BookWorm.Chassis.Search;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace BookWorm.Catalog.Infrastructure.GenAi;

public static class Extensions
{
    private const string ActivitySourceName = "Experimental.Microsoft.Extensions.AI";

    public static void AddGenAi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddOllamaApiClient(Components.Ollama.Embedding).AddEmbeddingGenerator();

        builder
            .AddOllamaApiClient(Components.Ollama.Chat)
            .AddChatClient()
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c =>
                c.EnableSensitiveData = builder.Environment.IsDevelopment()
            );

        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();
        services.AddScoped<ISearch, HybridSearch>();

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));
    }
}
