namespace BookWorm.McpTools.Extensions;

internal static class Extensions
{
    private const string ActivitySourceName = "Experimental.ModelContextProtocol";

    /// <summary>
    /// Configures core application services and middleware for the host builder, including CORS, exception handling, HTTP client references, MCP server setup, and OpenTelemetry instrumentation.
    /// </summary>
    /// <param name="builder">The application host builder to configure.</param>
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure HTTP client
        services.AddHttpServiceReference<ICatalogApi>(
            $"{Protocol.HttpOrHttps}://{Application.Catalog}",
            HealthStatus.Degraded
        );

        services
            .AddMcpServer(o =>
                o.ServerInfo = new() { Name = Application.McpTools, Version = "1.0.0" }
            )
            .WithHttpTransport()
            .WithToolsFromAssembly()
            .WithPromptsFromAssembly();

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));
    }
}
