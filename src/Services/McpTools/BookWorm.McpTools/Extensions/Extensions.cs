using AgentGovernance.Extensions.ModelContextProtocol;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Core;
using BookWorm.McpTools.Configurations;
using BookWorm.McpTools.Options;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Extensions;

internal static class Extensions
{
    private const string ActivitySourceName = "Experimental.ModelContextProtocol";

    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultCors();

            builder.AddAppSettings<McpToolsAppSettings>();

            builder.AddDefaultAuthentication().WithKeycloakClaimsTransformation();

            services
                .AddAuthorizationBuilder()
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireScope(
                            $"{Constants.Aspire.Services.McpTools}_{Authorization.Actions.Read}"
                        )
                        .Build()
                );

            services.AddHttpContextAccessor();

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

            // Agent governance (policy enforcement for MCP tool calls) wired directly into the
            // MCP pipeline via Microsoft.AgentGovernance.Extensions.ModelContextProtocol.
            services
                .AddMcpServer()
                .WithGovernance(o =>
                {
                    o.PolicyPaths.Add(
                        Path.Combine(
                            AppContext.BaseDirectory,
                            "AI",
                            "Governance",
                            "Policies",
                            "default.yaml"
                        )
                    );
                    o.PolicyPaths.Add(
                        Path.Combine(AppContext.BaseDirectory, "Policies", "mcp-tools.yaml")
                    );
                    o.RequireAuthenticatedAgentId = false;
                    o.DefaultAgentId = $"did:bookworm:{ServerInfoOptions.Name}";
                    o.ServerName = ServerInfoOptions.Name;
                })
                .WithHttpTransport(o => o.Stateless = true)
                .AddAuthorizationFilters()
                .WithToolsFromAssembly()
                .WithPromptsFromAssembly()
                .WithResourcesFromAssembly();

            builder.Configure<ServerInfoOptions>(ServerInfoOptions.ConfigurationSection);

            services
                .AddOptions<McpServerOptions>()
                .Configure(
                    (
                        McpServerOptions options,
                        IOptionsMonitor<ServerInfoOptions> serverInfoOptions
                    ) =>
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
                    }
                );

            services
                .AddOpenTelemetry()
                .WithMetrics(m => m.AddMeter(ActivitySourceName))
                .WithTracing(t => t.AddSource(ActivitySourceName));
        }
    }
}
