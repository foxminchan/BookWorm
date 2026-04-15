using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

namespace BookWorm.AppHost.Extensions.Network;

internal static class ProxyExtensions
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
                    for (var routeIndex = 0; routeIndex < services.Count; routeIndex++)
                    {
                        var service = services[routeIndex];
                        var routeBuilder = yarpBuilder.AddRoute(
                            $"/{service.Name}/{{**remainder}}",
                            service.Resource
                        );

                        if (service.UseProtobuf)
                        {
                            routeBuilder.WithTransformForwarded();
                        }

                        routeBuilder
                            .WithOrder(service.Order ?? routeIndex)
                            .WithMaxRequestBodySize(service.MaxRequestBodySize)
                            .WithTransformPathPrefix("/")
                            .WithTransformUseOriginalHostHeader()
                            .WithTransformPathRemovePrefix($"/{service.Name}")
                            .WithTransformXForwarded()
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

internal sealed class Service
{
    public const long DefaultMaxRequestBodySize = 10 * 1024 * 1024;
    public required string Name { get; init; }
    public required IResourceBuilder<ProjectResource> Resource { get; init; }
    public bool UseProtobuf { get; init; }
    public long MaxRequestBodySize { get; init; } = DefaultMaxRequestBodySize;
    public int? Order { get; init; }
}

internal sealed class ApiGatewayProxyBuilder
{
    private readonly List<Service> _services = [];

    internal ApiGatewayProxyBuilder(IDistributedApplicationBuilder builder)
    {
        Builder = builder;
    }

    private IDistributedApplicationBuilder Builder { get; }

    public ApiGatewayProxyBuilder WithService(
        IResourceBuilder<ProjectResource> service,
        bool useProtobuf = false,
        long maxRequestBodySize = Service.DefaultMaxRequestBodySize,
        int? order = null
    )
    {
        _services.Add(
            new()
            {
                Name = service.Resource.Name,
                Resource = service,
                UseProtobuf = useProtobuf,
                MaxRequestBodySize = maxRequestBodySize,
                Order = order,
            }
        );

        return this;
    }

    public IResourceBuilder<YarpResource> Build()
    {
        return Builder.BuildApiGatewayProxy(_services);
    }
}
