using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

namespace BookWorm.AppHost.Extensions.Network;

public static class ProxyExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public ApiGatewayProxyBuilder AddApiGatewayProxy()
        {
            return new(builder);
        }

        internal IResourceBuilder<YarpResource> BuildApiGatewayProxy(
            IReadOnlyList<Service> services
        )
        {
            var yarp = builder
                .AddYarp(Services.Gateway)
                .WithHttpsDeveloperCertificate()
                .WithIconName("SerialPort")
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
                })
                .WithExplicitStart();

            return yarp;
        }
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

    public IResourceBuilder<YarpResource> Build()
    {
        return Builder.BuildApiGatewayProxy(_services);
    }
}
