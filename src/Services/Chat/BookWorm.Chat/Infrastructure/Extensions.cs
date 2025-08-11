using BookWorm.Chassis.EF;
using BookWorm.Chassis.RAG;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.Chat.Infrastructure.ChatHistory;

namespace BookWorm.Chat.Infrastructure;

internal static class Extensions
{
    public static void AddPersistenceServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add database configuration
        builder.AddAzureNpgsqlDbContext<ChatDbContext>(Components.Database.Chat);
        services.AddMigration<ChatDbContext>();
        services.AddRepositories(typeof(IChatApiMarker));

        builder.AddRedisClient(Components.Redis);
    }

    public static void AddChatStreamingServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();
        builder.AddChatCompletion();
        builder.AddMcpClient();
        builder.AddAgents();

        services.AddSingleton<OrchestrateAgents>();
        services.AddScoped<IChatHistoryService, ChatHistoryService>();
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();
        services.AddScoped<IChatStreaming, ChatStreaming.ChatStreaming>();
    }

    private static void AddMcpClient(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpClient(
            Services.McpTools,
            client => client.BaseAddress = new($"{Protocols.HttpOrHttps}://{Services.McpTools}")
        );

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            var client = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(Services.McpTools);

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = Services.McpTools, Version = "1.0" },
            };

            SseClientTransportOptions sseTransportOptions = new()
            {
                Name = "AspNetCoreSseClient",
                TransportMode = HttpTransportMode.StreamableHttp,
                Endpoint = new(client.BaseAddress!, "mcp"),
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
