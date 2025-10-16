namespace BookWorm.McpTools.Extensions;

internal static class Extensions
{
    private const string ActivitySourceName = "Experimental.ModelContextProtocol";

    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var serviceProvider = services.BuildServiceProvider();
        var apiVersionDescriptions = serviceProvider.GetApiVersionDescription();

        builder.AddDefaultCors();

        // Add exception handlers
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure HTTP client
        services.AddHttpServiceReference<ICatalogApi>(
            $"{Protocols.HttpOrHttps}://{Constants.Aspire.Services.Catalog}",
            HealthStatus.Degraded
        );

        services
            .AddMcpServer(o =>
                o.ServerInfo = new()
                {
                    Name = Constants.Aspire.Services.McpTools,
                    Version = apiVersionDescriptions[0].ApiVersion.ToString(),
                }
            )
            .WithHttpTransport(o => o.Stateless = true)
            .WithToolsFromAssembly()
            .WithPromptsFromAssembly();

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));

        services.AddHttpContextAccessor().AddMcpOpenApi();
    }

    private static void AddMcpOpenApi(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        var document = sp.GetRequiredService<DocumentOptions>();

        foreach (var version in sp.GetApiVersionDescription())
        {
            services.AddOpenApi(
                version.GroupName,
                options =>
                {
                    options.ApplyApiVersionInfo(document, version);
                    options.AddDocumentTransformer<McpDocumentTransformer>();
                }
            );
        }
    }
}
