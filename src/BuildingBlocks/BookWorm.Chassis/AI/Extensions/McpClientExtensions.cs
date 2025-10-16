﻿using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class McpClientExtensions
{
    public static void AddMcpClient(
        this IHostApplicationBuilder builder,
        string serviceName,
        string relativePath = "/mcp"
    )
    {
        var services = builder.Services;

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            var url =
                $"{ServiceDiscoveryUtilities.GetServiceEndpoint(serviceName, Protocols.Https)}/{relativePath.TrimStart('/')}";

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
