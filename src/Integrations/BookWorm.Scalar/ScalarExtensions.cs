using System.Collections.Immutable;
using System.Diagnostics;
using Aspire.Hosting.Lifecycle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Forwarder;

namespace BookWorm.Scalar;

public static class ScalarExtensions
{
    /// <summary>
    ///     Adds a Scalar resource to the project resource builder.
    /// </summary>
    /// <param name="builder">The project resource builder.</param>
    /// <param name="documentNames">Optional array of document names. Defaults to ["v1"].</param>
    /// <param name="path">The path to the OpenAPI document. Defaults to "openapi/v1.json".</param>
    /// <param name="endpointName">The name of the endpoint. Defaults to "http".</param>
    /// <returns>The updated project resource builder with the Scalar resource added.</returns>
    public static IResourceBuilder<ProjectResource> WithScalar(
        this IResourceBuilder<ProjectResource> builder,
        string[]? documentNames = null,
        string path = "openapi/v1.json",
        string endpointName = "http"
    )
    {
        if (builder.ApplicationBuilder.Resources.OfType<ScalarResource>().Any())
        {
            return builder.WithAnnotation(
                new ScalarAnnotation(["v1"], "openapi/v1.json", builder.GetEndpoint("http"))
            );
        }

        builder.ApplicationBuilder.Services.TryAddLifecycleHook<ScalarLifecycleHook>();

        builder
            .ApplicationBuilder.AddResource(new ScalarResource("scalar-api-reference"))
            .WithInitialState(
                new()
                {
                    State = "Starting",
                    ResourceType = "scalar-api-reference",
                    Properties = [],
                }
            )
            .ExcludeFromManifest();

        return builder.WithAnnotation(
            new ScalarAnnotation(documentNames ?? ["v1"], path, builder.GetEndpoint(endpointName))
        );
    }

    private sealed class ScalarLifecycleHook(
        ResourceNotificationService resourceNotificationService,
        ResourceLoggerService resourceLoggerService
    ) : IDistributedApplicationLifecycleHook
    {
        public async Task AfterEndpointsAllocatedAsync(
            DistributedApplicationModel appModel,
            CancellationToken cancellationToken = default
        )
        {
            var scalarResource = appModel.Resources.OfType<ScalarResource>().SingleOrDefault();

            if (scalarResource is null)
            {
                return;
            }

            var builder = WebApplication.CreateSlimBuilder();

            builder.Services.AddHttpForwarder();
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(
                new ResourceLoggerProvider(resourceLoggerService.GetLogger(scalarResource.Name))
            );

            var app = builder.Build();

            app.MapScalarApiReference();

            var resourceToEndpoint = new Dictionary<string, (string, string)>();
            var portToResourceMap = new Dictionary<int, (string, List<string>)>();

            foreach (var resource in appModel.Resources)
            {
                if (!resource.TryGetLastAnnotation<ScalarAnnotation>(out var annotation))
                {
                    continue;
                }

                resourceToEndpoint[resource.Name] = (
                    annotation.EndpointReference.Url,
                    annotation.Route
                );

                List<string> paths = [];
                paths.AddRange(
                    annotation.DocumentNames.Select(documentName =>
                        $"scalar/{resource.Name}/{documentName}"
                    )
                );
                portToResourceMap[app.Urls.Count] = (annotation.EndpointReference.Url, paths);
                app.Urls.Add("http://127.0.0.1:0");
            }

            app.Map(
                "/openapi/{resourceName}/{documentName}.json",
                async (
                    string resourceName,
                    string documentName,
                    IHttpForwarder forwarder,
                    HttpContext context
                ) =>
                {
                    Debug.WriteLine($"Forwarding {context.Request.Path} with {documentName}");

                    var (endpoint, path) = resourceToEndpoint[resourceName];

                    using var client = new HttpMessageInvoker(new SocketsHttpHandler());

                    await forwarder.SendAsync(
                        context,
                        endpoint,
                        client,
                        (_, requestMessage) =>
                        {
                            requestMessage.RequestUri = new($"{endpoint}/{path}");
                            return ValueTask.CompletedTask;
                        }
                    );
                }
            );

            app.Map(
                "{*path}",
                async (HttpContext context, IHttpForwarder forwarder, string? path) =>
                {
                    var (endpoint, _) = portToResourceMap[context.Connection.LocalPort];

                    using var client = new HttpMessageInvoker(new SocketsHttpHandler());

                    await forwarder.SendAsync(
                        context,
                        endpoint,
                        client,
                        (_, r) =>
                        {
                            r.RequestUri = path is null
                                ? new(endpoint)
                                : new Uri($"{endpoint}/{path}");
                            return ValueTask.CompletedTask;
                        }
                    );
                }
            );

            await app.StartAsync(cancellationToken);

            var addresses = app
                .Services.GetRequiredService<IServer>()
                .Features.GetRequiredFeature<IServerAddressesFeature>()
                .Addresses;

            var urls = ImmutableArray.CreateBuilder<UrlSnapshot>();

            var index = 0;
            foreach (var rawAddress in addresses)
            {
                var address = BindingAddress.Parse(rawAddress);

                var (_, paths) = portToResourceMap[address.Port] = portToResourceMap[index++];

                foreach (var p in paths)
                {
                    urls.Add(new(rawAddress, $"{rawAddress}/{p}", false));
                }
            }

            await resourceNotificationService.PublishUpdateAsync(
                scalarResource,
                state => state with { State = "Running", Urls = urls.ToImmutable() }
            );
        }

        private sealed class ResourceLoggerProvider(ILogger logger) : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName)
            {
                return new ResourceLogger(logger);
            }

            public void Dispose() { }

            private sealed class ResourceLogger(ILogger logger) : ILogger
            {
                public IDisposable? BeginScope<TState>(TState state)
                    where TState : notnull
                {
                    return logger.BeginScope(state);
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return logger.IsEnabled(logLevel);
                }

                public void Log<TState>(
                    LogLevel logLevel,
                    EventId eventId,
                    TState state,
                    Exception? exception,
                    Func<TState, Exception?, string> formatter
                )
                {
                    logger.Log(logLevel, eventId, state, exception, formatter);
                }
            }
        }
    }
}
