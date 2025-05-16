namespace BookWorm.Chat.Extensions;

public static class Extensions
{
    private const string ActivitySourceName = "Experimental.Microsoft.Extensions.AI";

    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        builder.AddRedisClient(Components.Redis);

        services.AddSingleton<IChatStreaming, ChatStreaming>();
        services.AddSingleton<IConversationState, RedisConversationState>();
        services.AddSingleton<ICancellationManager, RedisCancellationManager>();

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        builder
            .AddOllamaApiClient(Components.Ollama.Chat)
            .AddChatClient()
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
