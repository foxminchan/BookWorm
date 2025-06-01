namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public static class Extensions
{
    private const string ActivitySourceName = "Experimental.Microsoft.Extensions.AI";

    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

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

        services.AddSingleton<IMcpClient>(_ =>
        {
            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = Application.McpTools, Version = "1.0.0" },
            };

            var client = new HttpClient();
            client.BaseAddress = new($"https+http://{Application.McpTools}");

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
