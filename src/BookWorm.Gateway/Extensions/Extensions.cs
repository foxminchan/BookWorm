using BookWorm.ServiceDefaults.Auth;
using BookWorm.ServiceDefaults.Kestrel;
using BookWorm.ServiceDefaults.Keycloak;

namespace BookWorm.Gateway.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        const string proxySection = "ReverseProxy";

        builder.AddDefaultCors();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddProblemDetails();

        services.AddHttpForwarderWithServiceDiscovery();

        services.AddRequestTimeouts();

        services.AddRateLimiting();

        services
            .AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection(proxySection))
            .AddServiceDiscoveryDestinationResolver();
    }
}
