using Aspire.Hosting.Dotnet;
using Aspire.Hosting.Yarp;

namespace BookWorm.AppHost.Extensions.Proxy;

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
            $"{entraId.Resource.Name}-proxy",
            entraId.Resource.Instance.TrimEnd('/')
        );
        return this;
    }

    public ApiGatewayProxyBuilder WithFrontend(
        IResourceBuilder<IResourceWithEndpoints> frontend,
        string? pathPrefix = null,
        int? order = null,
        string? zoneName = null
    )
    {
        _frontends.Add(
            new()
            {
                Resource = frontend,
                PathPrefix = pathPrefix,
                Order = order,
                ZoneName = zoneName,
            }
        );

        return this;
    }

    /// <summary>
    ///     Loads child microfrontend routes from a Vercel-style <c>microfrontends.json</c> file
    ///     and registers them on the YARP gateway with <c>X-Mfe-Zone</c> response headers.
    /// </summary>
    /// <param name="configPath">Path to <c>microfrontends.json</c>.</param>
    /// <param name="apps">Map from application key in the config file to its Aspire frontend resource.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ApiGatewayProxyBuilder WithMicrofrontends(
        string configPath,
        IReadOnlyDictionary<string, IResourceBuilder<IResourceWithEndpoints>> apps
    )
    {
        ArgumentNullException.ThrowIfNull(apps);

        var routes = MicrofrontendsConfigLoader.Load(configPath);

        foreach (var route in routes)
        {
            if (!apps.TryGetValue(route.ZoneName, out var resource))
            {
                throw new KeyNotFoundException(
                    $"Microfrontend application '{route.ZoneName}' defined in '{configPath}' was not provided to {nameof(WithMicrofrontends)}."
                );
            }

            _frontends.Add(
                new()
                {
                    Resource = resource,
                    Pattern = route.Pattern,
                    IsExact = route.IsExact,
                    ZoneName = route.ZoneName,
                }
            );
        }

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
