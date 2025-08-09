using A2A.AspNetCore;
using BookWorm.Chassis.RAG;
using BookWorm.Chassis.RAG.A2A;
using BookWorm.Constants.Aspire;
using ModelContextProtocol.Client;

namespace BookWorm.Agent.Rating.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultOpenApi();

        services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();

        builder.AddMcpClient();

        services.AddA2AClient(Agents.Summarize, $"{Protocols.HttpOrHttps}://{Agents.Summarize}");
    }

    public static void MapA2AEndpoints(this WebApplication app, string tag)
    {
        var hostAgent = new A2AHostAgent(
            AgentFactory
                .CreateAgentAsync(app.Services.GetRequiredService<Kernel>())
                .GetAwaiter()
                .GetResult(),
            AgentFactory.GetAgentCard()
        );

        app.MapA2A(hostAgent.TaskManager!, "/").WithTags(tag);

        app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags(tag);
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
                TransportMode = HttpTransportMode.StreamableHttp,
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
