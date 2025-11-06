using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.Security.Keycloak;

public static class KeycloakClaimsTransformationExtensions
{
    /// <summary>
    ///     Adds an <see cref="IClaimsTransformation" /> that transforms Keycloak resource access roles claims into regular
    ///     role claims.
    /// </summary>
    public static IHostApplicationBuilder WithKeycloakClaimsTransformation(
        this IHostApplicationBuilder builder
    )
    {
        builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
        return builder;
    }
}
