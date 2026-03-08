using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Security.Keycloak;

public static class Extensions
{
    public static IHostApplicationBuilder WithKeycloakClaimsTransformation(
        this IHostApplicationBuilder builder
    )
    {
        builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
        return builder;
    }

    public static IServiceCollection AddKeycloakTokenIntrospection(this IServiceCollection services)
    {
        return services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }

    public static IApplicationBuilder UseKeycloakTokenIntrospection(this IApplicationBuilder app)
    {
        return app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();
    }
}
