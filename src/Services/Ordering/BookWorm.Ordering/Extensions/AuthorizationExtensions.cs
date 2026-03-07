using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Ordering.Extensions;

internal static class AuthorizationExtensions
{
    public static void AddSecurityServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultAuthentication().WithKeycloakClaimsTransformation();

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                Authorization.Policies.Admin,
                policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Admin)
                        .RequireScope(
                            $"{Services.Ordering}_{Authorization.Actions.Read}",
                            $"{Services.Ordering}_{Authorization.Actions.Write}"
                        );
                }
            )
            .AddPolicy(
                Authorization.Policies.Reporter,
                policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Reporter)
                        .RequireScope($"{Services.Ordering}_{Authorization.Actions.Read}");
                }
            )
            .SetDefaultPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole(Authorization.Roles.User)
                    .RequireScope(
                        $"{Services.Ordering}_{Authorization.Actions.Read}",
                        $"{Services.Ordering}_{Authorization.Actions.Write}"
                    )
                    .Build()
            );

        // Configure ClaimsPrincipal
        services.AddTransient(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new ClaimsPrincipal()
        );

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
