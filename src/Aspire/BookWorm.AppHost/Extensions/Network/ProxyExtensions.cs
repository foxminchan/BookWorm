using Aspire.Hosting.Dotnet;
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

        internal static IResourceBuilder<YarpResource> ConfigureApiGatewayProxy(
            IResourceBuilder<YarpResource> yarp,
            IReadOnlyList<Service> services,
            IResourceBuilder<ExternalServiceResource>? entraExternal,
            IReadOnlyList<FrontendRoute> frontends
        )
        {
            return yarp.WithConfiguration(yarpBuilder =>
            {
                for (var routeIndex = 0; routeIndex < services.Count; routeIndex++)
                {
                    var service = services[routeIndex];
                    var routeBuilder = yarpBuilder.AddRoute(
                        $"/{service.Name}/{{**remainder}}",
                        service.Resource
                    );

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

                var nextOrder = services.Count;

                if (entraExternal is not null)
                {
                    yarpBuilder
                        .AddRoute("/entra/{**remainder}", entraExternal)
                        .WithOrder(nextOrder++)
                        .WithTransformPathRemovePrefix("/entra")
                        .WithTransformUseOriginalHostHeader(false);
                }

                foreach (var frontend in frontends)
                {
                    var pattern = string.IsNullOrEmpty(frontend.PathPrefix)
                        ? "/{**remainder}"
                        : $"{frontend.PathPrefix.TrimEnd('/')}/{{**remainder}}";

                    var order = frontend.Order
                        ?? (string.IsNullOrEmpty(frontend.PathPrefix) ? int.MaxValue : nextOrder++);

                    yarpBuilder
                        .AddRoute(pattern, frontend.Resource.GetEndpoint(Uri.UriSchemeHttp))
                        .WithOrder(order)
                        .WithTransformUseOriginalHostHeader()
                        .WithTransformXForwarded();
                }
            });
        }
    }
}

internal sealed class Service
{
    public const long DefaultMaxRequestBodySize = 10 * 1024 * 1024;
    public required string Name { get; init; }
    public required IResourceBuilder<DotnetProjectResource> Resource { get; init; }
    public long MaxRequestBodySize { get; init; } = DefaultMaxRequestBodySize;
    public int? Order { get; init; }
}

internal sealed class FrontendRoute
{
    public required IResourceBuilder<IResourceWithEndpoints> Resource { get; init; }
    public string? PathPrefix { get; init; }
    public int? Order { get; init; }
}

internal sealed class ApiGatewayProxyBuilder
{
    private readonly List<FrontendRoute> _frontends = [];
    private readonly List<Service> _services = [];
    private IResourceBuilder<ExternalServiceResource>? _entraExternal;

    internal ApiGatewayProxyBuilder(IDistributedApplicationBuilder builder)
    {
        Builder = builder;
        Resource = builder
            .AddYarp(Services.Gateway)
            .WithHttpsDeveloperCertificate()
            .WithExternalHttpEndpoints();
    }

    private IDistributedApplicationBuilder Builder { get; }

    public IResourceBuilder<YarpResource> Resource { get; }

    public ApiGatewayProxyBuilder WithService(
        IResourceBuilder<DotnetProjectResource> service,
        long maxRequestBodySize = Service.DefaultMaxRequestBodySize,
        int? order = null
    )
    {
        _services.Add(
            new()
            {
                Name = service.Resource.Name,
                Resource = service,
                MaxRequestBodySize = maxRequestBodySize,
                Order = order,
            }
        );

        return this;
    }

    public ApiGatewayProxyBuilder WithEntraProxy(
        IResourceBuilder<EntraIdApplicationResource> entraId
    )
    {
        _entraExternal = Builder.AddExternalService(
            "entra",
            entraId.Resource.Instance.TrimEnd('/')
        );
        return this;
    }

    public ApiGatewayProxyBuilder WithFrontend(
        IResourceBuilder<IResourceWithEndpoints> frontend,
        string? pathPrefix = null,
        int? order = null
    )
    {
        _frontends.Add(
            new()
            {
                Resource = frontend,
                PathPrefix = pathPrefix,
                Order = order,
            }
        );

        return this;
    }

    public IResourceBuilder<YarpResource> Build()
    {
        foreach (var service in _services)
        {
            service.Resource.WithGatewayCors(Resource);
        }

        return IDistributedApplicationBuilder.ConfigureApiGatewayProxy(
            Resource,
            _services,
            _entraExternal,
            _frontends
        );
    }
}
