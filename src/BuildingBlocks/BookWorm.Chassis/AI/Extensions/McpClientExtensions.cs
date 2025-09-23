using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class McpClientExtensions
{
    public static void AddMcpClient(
        this IHostApplicationBuilder builder,
        string clientName,
        string serverUrl,
        string relativePath = "mcp",
        string version = "1.0"
    )
    {
        var services = builder.Services;

        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(serverUrl);

        if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "Server URL must be a valid absolute URI.",
                nameof(serverUrl)
            );
        }

        services.AddHttpClient(clientName, client => client.BaseAddress = uri);

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new() { Name = clientName, Version = version },
            };

            HttpClientTransportOptions transportOptions = new()
            {
                Name = $"{clientName}SseClient",
                TransportMode = HttpTransportMode.StreamableHttp,
                Endpoint = client.BaseAddress is not null
                    ? new(client.BaseAddress, relativePath)
                    : throw new InvalidOperationException(
                        $"HttpClient for '{clientName}' has no BaseAddress configured"
                    ),
            };

            HttpClientTransport transport = new(transportOptions, loggerFactory);

            return McpClient
                .CreateAsync(transport, mcpClientOptions, loggerFactory)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        });
    }

    public static async Task<List<ChatMessage>> MapToChatMessagesAsync(this McpClient mcpClient)
    {
        var prompts = await mcpClient.ListPromptsAsync().ConfigureAwait(false);

        List<ChatMessage> promptMessages = [];

        foreach (var prompt in prompts)
        {
            var chatMessages = await prompt.GetAsync().ConfigureAwait(false);
            promptMessages.AddRange(chatMessages.ToChatMessages());
        }

        return promptMessages;
    }
}
