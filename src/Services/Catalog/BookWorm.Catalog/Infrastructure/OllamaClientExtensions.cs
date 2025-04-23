namespace BookWorm.Catalog.Infrastructure;

public static class OllamaClientExtensions
{
    private const string ActivitySourceName = "Experimental.Microsoft.Extensions.AI";

    public static void AddOllamaClient(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder
            .AddOllamaApiClient(Components.Ollama.Embedding)
            .AddEmbeddingGenerator()
            .UseLogging();

        builder
            .AddOllamaApiClient(Components.Ollama.Chat)
            .AddChatClient()
            .UseFunctionInvocation()
            .UseLogging();

        services.AddSingleton<IAiService, AiService>();
        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);
        services.AddSingleton<IChatStreaming, ChatStreaming>();
        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));
    }
}
