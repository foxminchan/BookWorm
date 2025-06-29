using BookWorm.Chassis.AI;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public static class Extensions
{
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

        builder.AddAITelemetry();

        builder.AddChatClient();

        services.AddMcpClient();

        services.AddSingleton<IChatStreaming, ChatStreaming>();
    }

    private static void AddMcpClient(this IServiceCollection services)
    {
        services.AddTransient(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = Application.McpTools, Version = "1.0.0" },
            };

            var client = new HttpClient
            {
                BaseAddress = new($"{Protocol.HttpOrHttps}://{Application.McpTools}"),
            };

            SseClientTransportOptions sseTransportOptions = new()
            {
                Name = "AspNetCoreSse",
                Endpoint = client.BaseAddress,
            };

            SseClientTransport sseClientTransport = new(sseTransportOptions, loggerFactory);

            var mcpClient = McpClientFactory
                .CreateAsync(sseClientTransport, mcpClientOptions, loggerFactory)
                .GetAwaiter()
                .GetResult();

            return mcpClient;
        });
    }
}
