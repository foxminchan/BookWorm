using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Chat.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

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
                            $"{Services.Chatting}_{Authorization.Actions.Read}",
                            $"{Services.Chatting}_{Authorization.Actions.Write}"
                        );
                }
            )
            .SetDefaultPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole(Authorization.Roles.User)
                    .RequireScope(
                        $"{Services.Chatting}_{Authorization.Actions.Read}",
                        $"{Services.Chatting}_{Authorization.Actions.Write}"
                    )
                    .Build()
            );

        builder.AddDefaultOpenApi();

        // Add exception handlers
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddRateLimiting();

        services.AddVersioning();
        services.AddEndpoints(typeof(IChatApiMarker));

        // Configure ClaimsPrincipal
        services.AddTransient(s => s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

        builder.AddAIAgentsServices();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
