namespace BookWorm.Catalog.Infrastructure;

public static class OllamaClientExtensions
{
    public static void AddOllamaClient(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddOllamaApiClient(Components.Ollama.Embedding).AddEmbeddingGenerator();
        builder.AddOllamaApiClient(Components.Ollama.Chat).AddChatClient().UseFunctionInvocation();

        services.AddSingleton<IAiService, AiService>();

        services.AddSignalR();
        services.AddSingleton<IChatStreaming, ChatStreaming>();
        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter("Experimental.Microsoft.Extensions.AI"))
            .WithTracing(t => t.AddSource("Experimental.Microsoft.Extensions.AI"));
    }
}
