using BookWorm.Chassis.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class McpClientExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers a singleton <see cref="McpClient" /> for the specified service using StreamableHttp transport.
        /// </summary>
        /// <param name="serviceName">
        ///     The logical service name used for service discovery and transport naming.
        /// </param>
        /// <param name="relativePath">
        ///     The relative URL path for the MCP endpoint. Defaults to <c>/mcp</c>.
        /// </param>
        /// <example>
        ///     <code>
        ///         builder.AddMcpClient("catalog-service");
        ///         builder.AddMcpClient("rating-service", "/api/mcp");
        ///     </code>
        /// </example>
        public void AddMcpClient(string serviceName, string relativePath = "/mcp")
        {
            var services = builder.Services;

            services.AddSingleton(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var url = HttpUtilities
                    .AsUrlBuilder()
                    .WithBase(ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(serviceName))
                    .WithPath(relativePath)
                    .Build();

                HttpClientTransportOptions transportOptions = new()
                {
                    Name = $"{serviceName}-Transport",
                    TransportMode = HttpTransportMode.StreamableHttp,
                    Endpoint = new(url),
                };

                HttpClientTransport transport = new(transportOptions, loggerFactory);

                return McpClient
                    .CreateAsync(transport, loggerFactory: loggerFactory)
                    .GetAwaiter()
                    .GetResult();
            });
        }
    }
}
