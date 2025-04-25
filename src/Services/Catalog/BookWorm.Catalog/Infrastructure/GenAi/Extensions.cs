using BookWorm.Catalog.Infrastructure.GenAi.CancellationManager;
using BookWorm.Catalog.Infrastructure.GenAi.ChatStreaming;
using BookWorm.Catalog.Infrastructure.GenAi.ConversationState;
using BookWorm.Catalog.Infrastructure.GenAi.ConversationState.Abstractions;
using BookWorm.Catalog.Infrastructure.GenAi.Ingestion;
using BookWorm.Catalog.Infrastructure.GenAi.Search;
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
        services.AddSingleton<IChatStreaming, ChatStreaming.ChatStreaming>();
        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();

        services.AddScoped<IIngestionSource<Book>, BookDataIngestor>();
        services.AddScoped<ISearch, HybridSearch>();

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));
    }
}
