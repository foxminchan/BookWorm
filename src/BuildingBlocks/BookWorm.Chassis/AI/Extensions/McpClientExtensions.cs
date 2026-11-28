using BookWorm.Chassis.AI.Skills;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Chassis.Utilities;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class McpClientExtensions
{
    extension(McpClient mcpClient)
    {
        /// <summary>
        ///     Loads the MCP tools declared by an agent definition.
        /// </summary>
        /// <param name="toolNames">The declared tool names allowed for the agent.</param>
        /// <returns>The matching MCP tools.</returns>
        /// <exception cref="InvalidDataException">
        ///     The definition declares no tools or the MCP server provides no matching tools.
        /// </exception>
        public AITool[] GetAgentTools(IReadOnlySet<string> toolNames)
        {
            ArgumentNullException.ThrowIfNull(mcpClient);
            ArgumentNullException.ThrowIfNull(toolNames);

            if (toolNames.Count == 0)
            {
                throw new InvalidDataException("Agent definition must declare MCP tools.");
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var tools = mcpClient
                .ListToolsAsync(cancellationToken: cts.Token)
                .Preserve()
                .GetAwaiter()
                .GetResult()
                .Where(tool => toolNames.Contains(tool.Name))
                .ToArray<AITool>();

            if (tools.Length == 0)
            {
                throw new InvalidDataException(
                    "MCP server did not provide any tools declared by the agent definition."
                );
            }

            return tools;
        }

        /// <summary>
        ///     Creates an <see cref="AgentSkillsProvider" /> backed by the MCP server's Skills extension (SEP-2640).
        /// </summary>
        /// <param name="skillNames">
        ///     Optional set of allowed skill names to expose to the agent. When <see langword="null" />, all skills
        ///     published by the MCP server are exposed.
        /// </param>
        /// <param name="loggerFactory">Optional logger factory for skill discovery and loading diagnostics.</param>
        /// <returns>An <see cref="AgentSkillsProvider" /> that loads and verifies skills over MCP.</returns>
        public AgentSkillsProvider CreateSkillsProvider(
            IReadOnlySet<string>? skillNames = null,
            ILoggerFactory? loggerFactory = null
        )
        {
            ArgumentNullException.ThrowIfNull(mcpClient);

            var builder = new AgentSkillsProviderBuilder().UseSource(
                new McpAgentSkillsSource(mcpClient, loggerFactory)
            );

            if (skillNames is { Count: > 0 })
            {
                builder.UseFilter((skill, _) => skillNames.Contains(skill.Frontmatter.Name));
            }

            if (loggerFactory is not null)
            {
                builder.UseLoggerFactory(loggerFactory);
            }

            return builder.Build();
        }
    }

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
            var transportName = $"{serviceName}-Transport";

            services.AddHttpContextAccessor();
            services.AddHttpClient(transportName).AddAgentTokenExchange(serviceName);

            services.AddSingleton(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

                var url = HttpUtilities
                    .AsUrlBuilder()
                    .WithBase(ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(serviceName))
                    .WithPath(relativePath)
                    .Build();

                HttpClientTransportOptions transportOptions = new()
                {
                    Name = transportName,
                    TransportMode = HttpTransportMode.StreamableHttp,
                    Endpoint = new(url),
                };

                HttpClientTransport transport = new(
                    transportOptions,
                    httpClientFactory.CreateClient(transportName),
                    loggerFactory,
                    true
                );

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                return McpClient
                    .CreateAsync(
                        transport,
                        loggerFactory: loggerFactory,
                        cancellationToken: cts.Token
                    )
                    .GetAwaiter()
                    .GetResult();
            });
        }
    }
}
