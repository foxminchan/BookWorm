using BookWorm.Chassis.AI;
using BookWorm.Chat.Agents;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public static class Extensions
{
    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Register chat context composite service
        services.AddSingleton<ChatContext>();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();

        builder.AddMcpClient();

        builder.AddAgents();

        services.AddSingleton<IChatStreaming, ChatStreaming>();
    }

    private static void AddMcpClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient(async sp =>
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

            var mcpClient = await McpClientFactory.CreateAsync(
                sseClientTransport,
                mcpClientOptions,
                loggerFactory
            );

            return mcpClient;
        });
    }
}
