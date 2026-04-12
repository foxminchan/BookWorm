using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Core;
using BookWorm.McpTools.Configurations;
using BookWorm.McpTools.Options;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.Extensions.Options;
using ModelContextProtocol;
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

        builder.AddAppSettings<McpToolsAppSettings>();

        // Add exception handlers
        services.AddGlobalExceptionHandler();
        services.AddProblemDetails();

        // Configure HTTP clients
        services.AddHttpServiceReference<ICatalogApi>(
            HttpUtilities
                .AsUrlBuilder()
                .WithScheme(Http.Schemes.HttpOrHttps)
                .WithHost(Constants.Aspire.Services.Catalog)
                .Build(),
            HealthStatus.Degraded
        );

        services.AddHttpServiceReference<IRatingApi>(
            HttpUtilities
                .AsUrlBuilder()
                .WithScheme(Http.Schemes.HttpOrHttps)
                .WithHost(Constants.Aspire.Services.Rating)
                .Build(),
            HealthStatus.Degraded
        );

        // Agent governance (policy enforcement for MCP tool calls)
        builder.AddAgentGovernance("Policies/mcp-tools.yaml");

        services
            .AddMcpServer()
            .WithHttpTransport(o => o.Stateless = true)
            .WithToolsFromAssembly()
            .WithPromptsFromAssembly()
            .WithResourcesFromAssembly()
            .WithSetLoggingLevelHandler(
                async (ctx, ct) =>
                {
                    if (ctx.Params?.Level is null)
                    {
                        throw new McpProtocolException(
                            "Missing required argument 'level'",
                            McpErrorCode.InvalidParams
                        );
                    }

                    await ctx.Server.SendNotificationAsync(
                        "notifications/message",
                        new
                        {
                            Level = nameof(LogLevel.Debug).ToLowerInvariant(),
                            Logger = ServerInfoOptions.Name,
                            Data = $"Logging level set to {ctx.Params.Level}",
                        },
                        cancellationToken: ct
                    );

                    return new();
                }
            );

        builder.Configure<ServerInfoOptions>(ServerInfoOptions.ConfigurationSection);

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

                    options.ServerInstructions = """
                    This is the BookWorm MCP server. It exposes tools and resources for
                    interacting with the BookWorm bookstore platform.

                    Available capabilities:
                    - search_catalog: Search books by description or keyword
                    - get_book: Retrieve full details for a specific book by ID
                    - list_categories: List all book categories
                    - list_authors: List all authors in the catalog
                    - get_book_reviews: Retrieve customer reviews for a specific book

                    Resources (stable URIs for ambient context):
                    - bookworm://catalog/categories — all book categories
                    - bookworm://catalog/authors — all authors
                    - bookworm://catalog/books/{id} — single book details
                    - bookworm://ratings/{bookId}/reviews — reviews for a book

                    Prompts:
                    - recommend_books: Generate a structured recommendation request
                    - analyze_book_quality: Classify a book as Best Seller / Good / Bad / No Data
                    """;

                    options.Capabilities = new() { Logging = new() };
                }
            );

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));

        services
            .AddHttpContextAccessor()
            .AddDefaultOpenApi(options =>
            {
                options.AddDocumentTransformer<
                    OpenApiInfoDefinitionsTransformer<McpToolsAppSettings>
                >();
                options.AddDocumentTransformer<McpDocumentTransformer>();
            });
    }
}
