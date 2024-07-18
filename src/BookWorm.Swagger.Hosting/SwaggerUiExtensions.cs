using System.Collections.Immutable;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Forwarder;

namespace BookWorm.Swagger.Hosting;

public static class SwaggerUiExtensions
{
    public static IResourceBuilder<ProjectResource> WithSwaggerUi(this IResourceBuilder<ProjectResource> builder,
        string[]? documentNames = null, string path = "swagger/v1/swagger.json", string endpointName = "https")
    {
        if (builder.ApplicationBuilder.Resources.OfType<SwaggerUiResource>().Any())
        {
            return builder.WithAnnotation(new SwaggerUiAnnotation(documentNames ?? ["v1"], path,
                builder.GetEndpoint(endpointName)));
        }

        builder.ApplicationBuilder.Services.TryAddLifecycleHook<SwaggerUiHook>();
        builder.ApplicationBuilder.AddResource(new SwaggerUiResource("swagger-ui"))
            .WithInitialState(new() { ResourceType = "swagger-ui", Properties = [], State = "Starting" })
            .ExcludeFromManifest();

        return builder.WithAnnotation(new SwaggerUiAnnotation(documentNames ?? ["v1"], path,
            builder.GetEndpoint(endpointName)));
    }

    internal sealed class SwaggerUiHook(
        ResourceNotificationService notificationService,
        ResourceLoggerService resourceLoggerService) : IDistributedApplicationLifecycleHook
    {
        public async Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel,
            CancellationToken cancellationToken = default)
        {
            var openApiResource = appModel.Resources.OfType<SwaggerUiResource>().SingleOrDefault();

            if (openApiResource is null)
            {
                return;
            }

            var builder = WebApplication.CreateSlimBuilder();

            builder.Services.AddHttpForwarder();
            builder.Logging.ClearProviders();

            builder.Logging.AddProvider(
                new ResourceLoggerProvider(resourceLoggerService.GetLogger(openApiResource.Name)));

            var app = builder.Build();

            app.MapSwaggerUi();

            var resourceToEndpoint = new Dictionary<string, (string, string)>();
            var portToResourceMap = new Dictionary<int, (string, List<string>)>();

            foreach (var r in appModel.Resources)
            {
                if (!r.TryGetLastAnnotation<SwaggerUiAnnotation>(out var annotation))
                {
                    continue;
                }

                resourceToEndpoint[r.Name] = (annotation.EndpointReference.Url, annotation.Path);

                var paths = annotation.DocumentNames.Select(documentName => $"swagger/{r.Name}/{documentName}")
                    .ToList();

                portToResourceMap[app.Urls.Count] = (annotation.EndpointReference.Url, paths);

                app.Urls.Add("http://127.0.0.1:0");
            }

            var handler = new SocketsHttpHandler
            {
                SslOptions = new()
                {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                }
            };
            var client = new HttpMessageInvoker(handler);

            app.Map("/openapi/{resourceName}/{documentName}.json",
                async (string resourceName, string documentName, IHttpForwarder forwarder, HttpContext context) =>
                {
                    Console.WriteLine($"Forwarding request to {resourceName}/{documentName}.json");

                    var (endpoint, path) = resourceToEndpoint[resourceName];

                    await forwarder.SendAsync(context, endpoint, client, (_, r) =>
                    {
                        r.RequestUri = new($"{endpoint}/{path}");
                        return ValueTask.CompletedTask;
                    });
                });

            app.Map("{*path}", async (HttpContext context, IHttpForwarder forwarder, string? path) =>
            {
                var (endpoint, _) = portToResourceMap[context.Connection.LocalPort];

                await forwarder.SendAsync(context, endpoint, client, (_, r) =>
                {
                    r.RequestUri = path is null ? new(endpoint) : new($"{endpoint}/{path}");
                    return ValueTask.CompletedTask;
                });
            });

            await app.StartAsync(cancellationToken);

            var addresses = app.Services.GetRequiredService<IServer>().Features
                .GetRequiredFeature<IServerAddressesFeature>().Addresses;

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

            await notificationService.PublishUpdateAsync(openApiResource,
                s => s with { State = "Running", Urls = urls.ToImmutableArray() });
        }
    }

    private sealed class ResourceLoggerProvider(ILogger logger) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ResourceLogger(logger);
        }

        public void Dispose()
        {
        }

        private class ResourceLogger(ILogger logger) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return logger.BeginScope(state);
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logger.IsEnabled(logLevel);
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}
