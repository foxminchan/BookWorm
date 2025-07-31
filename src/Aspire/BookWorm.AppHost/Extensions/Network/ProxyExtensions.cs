using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

namespace BookWorm.AppHost.Extensions.Network;

public static class ProxyExtensions
{
    public static ApiGatewayProxyBuilder AddApiGatewayProxy(
        this IDistributedApplicationBuilder builder
    )
    {
        return new(builder);
    }

    internal static IResourceBuilder<YarpResource> BuildApiGatewayProxy(
        this IDistributedApplicationBuilder builder,
        IReadOnlyList<Service> services,
        IResourceBuilder<ContainerResource> container
    )
    {
        var yarp = builder
            .AddYarp(Services.Gateway)
            .WithExternalHttpEndpoints()
            .WithConfiguration(yarpBuilder =>
            {
                foreach (var service in services)
                {
                    var routeBuilder = yarpBuilder.AddRoute(
                        $"/{service.Name}/{{**remainder}}",
                        service.Resource
                    );

                    if (service.UseProtobuf)
                    {
                        routeBuilder.WithTransformForwarded();
                    }

                    routeBuilder
                        .WithTransformPathPrefix("/")
                        .WithTransformUseOriginalHostHeader()
                        .WithTransformPathRemovePrefix($"/{service.Name}")
                        .WithTransformXForwarded("trace-id")
                        .WithTransformXForwarded("Trace-Id")
                        .WithTransformResponseHeader(
                            "X-Powered-By",
                            $"{nameof(BookWorm)} {nameof(Services.Gateway)}"
                        );
                }

                yarpBuilder.AddRoute(
                    "/identity/{**remainder}",
                    container.GetEndpoint(Protocols.Http)
                );
            });

        return yarp;
    }
}

public sealed class Service
{
    public required string Name { get; init; }
    public required IResourceBuilder<ProjectResource> Resource { get; init; }
    public bool UseProtobuf { get; init; }
}

public sealed class ApiGatewayProxyBuilder
{
    private readonly List<Service> _services = [];
    private IResourceBuilder<ContainerResource>? _container;

    internal ApiGatewayProxyBuilder(IDistributedApplicationBuilder builder)
    {
        Builder = builder;
    }

    private IDistributedApplicationBuilder Builder { get; }

    public ApiGatewayProxyBuilder WithService(
        IResourceBuilder<ProjectResource> service,
        bool useProtobuf = false
    )
    {
        _services.Add(
            new()
            {
                Name = service.Resource.Name,
                Resource = service,
                UseProtobuf = useProtobuf,
            }
        );
        return this;
    }

    public IResourceBuilder<YarpResource> WithService(IResourceBuilder<ContainerResource> container)
    {
        _container = container;
        return Build();
    }

    private IResourceBuilder<YarpResource> Build()
    {
        if (_container is null)
        {
            throw new InvalidOperationException("Container resource must be set before building.");
        }

        return Builder.BuildApiGatewayProxy(_services, _container);
    }
}
