using System.Security.Claims;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Chat.Configurations;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Chat.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddAppSettings<ChatAppSettings>();

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

        // Add exception handlers
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        builder.AddRateLimiting();

        services.AddVersioning();
        services.AddEndpoints(typeof(IChatApiMarker));
        services.AddDefaultOpenApi(options =>
            options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer<ChatAppSettings>>()
        );

        // Configure ClaimsPrincipal
        services.AddTransient(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new ClaimsPrincipal()
        );

        builder.AddAIAgentsServices();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
