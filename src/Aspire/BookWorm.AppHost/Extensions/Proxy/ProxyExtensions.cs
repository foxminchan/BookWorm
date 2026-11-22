using Aspire.Hosting.Yarp;
using Aspire.Hosting.Yarp.Transforms;

namespace BookWorm.AppHost.Extensions.Proxy;

internal static class ProxyExtensions
{
    extension(IResourceBuilder<YarpResource> builder)
    {
        public EndpointReference GetBrowserEndpoint()
        {
            var scheme = builder.ApplicationBuilder.ExecutionContext.IsRunMode
                ? Uri.UriSchemeHttps
                : Uri.UriSchemeHttp;

            return builder.GetEndpoint(scheme);
        }
    }

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

                var serviceNames = services
                    .Select(s => s.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var frontend in frontends)
                {
                    var pattern = ResolveFrontendPattern(frontend);
                    var order = ResolveFrontendOrder(
                        frontend,
                        pattern,
                        serviceNames,
                        ref nextOrder
                    );

                    var routeBuilder = yarpBuilder
                        .AddRoute(pattern, frontend.Resource.GetEndpoint(Uri.UriSchemeHttp))
                        .WithOrder(order)
                        .WithTransformUseOriginalHostHeader()
                        .WithTransformXForwarded();

                    if (!string.IsNullOrWhiteSpace(frontend.ZoneName))
                    {
                        routeBuilder.WithTransformResponseHeader("X-Mfe-Zone", frontend.ZoneName);
                    }
                }
            });
        }
    }

    internal static string ResolveFrontendPattern(FrontendRoute frontend)
    {
        if (!string.IsNullOrEmpty(frontend.Pattern))
        {
            return frontend.Pattern;
        }

        if (string.IsNullOrEmpty(frontend.PathPrefix))
        {
            return "/{**remainder}";
        }

        var trimmedPrefix = frontend.PathPrefix.TrimEnd('/');

        if (frontend.IsExact)
        {
            return string.IsNullOrEmpty(trimmedPrefix) ? "/" : trimmedPrefix;
        }

        return $"{trimmedPrefix}/{{**remainder}}";
    }

    internal static int ResolveFrontendOrder(
        FrontendRoute frontend,
        string pattern,
        IReadOnlySet<string> serviceNames,
        ref int nextOrder
    )
    {
        if (frontend.Order is not null)
        {
            return frontend.Order.Value;
        }

        if (pattern == "/{**remainder}")
        {
            return int.MaxValue;
        }

        if (
            frontend.IsExact
            && MicrofrontendsConfigLoader.CollidesWithService(pattern, serviceNames)
        )
        {
            return -1;
        }

        return nextOrder++;
    }
}
