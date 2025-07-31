using BookWorm.Chassis.RAG;
using BookWorm.Chat.Agents;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

internal static class Extensions
{
    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();

        builder.AddMcpClient();

        builder.AddAgents();

        services.AddSingleton<ChatAgents>();

        services.AddSingleton<IChatStreaming, ChatStreaming>();
    }

    private static void AddMcpClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMcpClient>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = Services.McpTools, Version = "1.0" },
            };

            var client = new HttpClient
            {
                BaseAddress = new($"{Protocols.HttpOrHttps}://{Services.McpTools}"),
            };

            SseClientTransportOptions sseTransportOptions = new()
            {
                Name = "AspNetCoreSse",
                Endpoint = new(client.BaseAddress, "mcp"),
            };

            SseClientTransport sseClientTransport = new(sseTransportOptions, loggerFactory);

            // Since this is synchronous DI registration, we need to use synchronous method
            // or provide a lazy initialization pattern
            var mcpClient = McpClientFactory
                .CreateAsync(sseClientTransport, mcpClientOptions, loggerFactory)
                .GetAwaiter()
                .GetResult();

            return mcpClient;
        });
    }
}
