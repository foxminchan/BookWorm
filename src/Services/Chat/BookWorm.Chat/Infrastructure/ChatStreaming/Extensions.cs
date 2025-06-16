namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public static class Extensions
{
    private const string ActivitySourceName = "Experimental.Microsoft.Extensions.AI";

    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Register chat context composite service
        services.AddSingleton(provider =>
        {
            var conversationState = provider.GetRequiredService<IConversationState>();
            var cancellationManager = provider.GetRequiredService<ICancellationManager>();
            return new ChatContext(conversationState, cancellationManager);
        });

        services.AddSingleton<IChatStreaming, ChatStreaming>();

        builder
            .AddOllamaApiClient(Components.Ollama.Chat)
            .AddChatClient()
            .UseDistributedCache()
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c =>
                c.EnableSensitiveData = builder.Environment.IsDevelopment()
            );

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));

        services.AddSingleton(_ =>
        {
            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = Application.McpTools, Version = "1.0.0" },
            };

            var client = new HttpClient
            {
                BaseAddress = new($"{Protocol.HttpOrHttps}://{Application.McpTools}"),
            };

            SseClientTransportOptions sseTransportOptions = new() { Endpoint = client.BaseAddress };

            SseClientTransport sseClientTransport = new(sseTransportOptions);

            var mcpClient = McpClientFactory
                .CreateAsync(sseClientTransport, mcpClientOptions)
                .GetAwaiter()
                .GetResult();

            return mcpClient;
        });
    }
}
