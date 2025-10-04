using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class McpClientExtensions
{
    public static void AddMcpClient(
        this IHostApplicationBuilder builder,
        string clientName,
        string endpoint,
        string relativePath = "mcp",
        string version = "1.0"
    )
    {
        var services = builder.Services;

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = clientName, Version = version },
            };

            var name = $"services__{clientName}__{endpoint}__0";
            var url = $"{Environment.GetEnvironmentVariable(name)}/{relativePath}";

            HttpClientTransportOptions transportOptions = new()
            {
                Name = $"{clientName}-Transport",
                TransportMode = HttpTransportMode.StreamableHttp,
                Endpoint = new(url),
            };

            HttpClientTransport transport = new(transportOptions, loggerFactory);

            return McpClient
                .CreateAsync(transport, mcpClientOptions, loggerFactory)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        });
    }
}
