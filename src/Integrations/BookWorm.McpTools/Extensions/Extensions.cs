using BookWorm.McpTools.Options;
using BookWorm.ServiceDefaults.Configuration;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Extensions;

internal static class Extensions
{
    private const string ActivitySourceName = "Experimental.ModelContextProtocol";

    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

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
            .AddMcpServer()
            .WithHttpTransport(o => o.Stateless = true)
            .WithToolsFromAssembly()
            .WithPromptsFromAssembly();

        services.Configure<ServerInfoOptions>(ServerInfoOptions.ConfigurationSection);

        services
            .AddOptions<McpServerOptions>()
            .Configure(
                (McpServerOptions options, IOptionsMonitor<ServerInfoOptions> serverInfoOptions) =>
                {
                    var value = serverInfoOptions.CurrentValue;
                    options.ServerInfo = new()
                    {
                        Name = value.Name,
                        Version = value.Version,
                        Title = value.Title,
                        WebsiteUrl = value.WebsiteUrl,
                        Icons = value
                            .Icons?.Select(i => new Icon
                            {
                                Source = i.Src,
                                MimeType = i.MimeType,
                                Sizes = i.Sizes,
                            })
                            .ToList(),
                    };
                }
            );

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
