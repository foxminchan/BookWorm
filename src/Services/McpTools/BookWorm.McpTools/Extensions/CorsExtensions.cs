using BookWorm.Constants.Core;
using Microsoft.Net.Http.Headers;

namespace BookWorm.McpTools.Extensions;

internal static class CorsExtensions
{
    private const string McpBrowserClientPolicy = "McpBrowserClient";

    extension(IHostApplicationBuilder builder)
    {
        public void AddMcpCors()
        {
            string[]? allowedOrigins = null;

            if (!builder.Environment.IsDevelopment())
            {
                allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

                if (allowedOrigins is not { Length: > 0 })
                {
                    throw new InvalidOperationException(
                        "Failed to bind CORS allowed origins from configuration section: Cors:Origins"
                    );
                }
            }

            // Only enable CORS if you intentionally want browser-based cross-origin access to this server.
            // Keep the allowlist narrowly scoped to known origins. Broad CORS settings weaken security.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(McpBrowserClientPolicy, policy =>
                {
                    if (allowedOrigins is not null)
                    {
                        policy.WithOrigins(allowedOrigins);
                    }
                    else
                    {
                        policy.SetIsOriginAllowed(Network.IsLocalhostOrigin);
                    }

                    policy
                        .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Delete)
                        // Browsers can send Accept without extra CORS configuration. These are the MCP-specific
                        // and non-safe listed headers the browser-based client needs for stateful Streamable HTTP.
                        .WithHeaders(
                            HeaderNames.ContentType,
                            "MCP-Protocol-Version",
                            "Mcp-Session-Id"
                        )
                        .WithExposedHeaders("Mcp-Session-Id");
                });
            });
        }
    }

    extension(WebApplication app)
    {
        public void UseMcpCors()
        {
            app.UseCors(McpBrowserClientPolicy);
        }
    }
}
