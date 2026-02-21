using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configuration;
using BookWorm.Constants.Core;
using BookWorm.McpTools.Options;
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
            HttpUtilities
                .AsUrlBuilder()
                .WithScheme(Http.Schemes.HttpOrHttps)
                .WithHost(Constants.Aspire.Services.Catalog)
                .Build(),
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
                        Name = ServerInfoOptions.Name,
                        Version = value.Version,
                        Title = value.Title,
                        WebsiteUrl = value.WebsiteUrl,
                        Icons = value
                            .Icons?.Select(i => new Icon
                            {
                                Source = i.Src,
                                MimeType = i.MimeType,
                                Sizes = i.Sizes?.ToList(),
                            })
                            .ToList(),
                    };
                }
            );

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));

        services
            .AddHttpContextAccessor()
            .AddDefaultOpenApi(options => options.AddDocumentTransformer<McpDocumentTransformer>());
    }
}
