using BookWorm.Chassis;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Chat.Infrastructure.Backplane;
using BookWorm.SharedKernel;
using Mediator;
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
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure Mediator
        services.AddMediator(
            (MediatorOptions options) =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;

                options.Assemblies =
                [
                    typeof(ISharedKernelMarker),
                    typeof(IChassisMarker),
                    typeof(IChatApiMarker),
                ];

                options.PipelineBehaviors =
                [
                    typeof(ActivityBehavior<,>),
                    typeof(LoggingBehavior<,>),
                    typeof(ValidationBehavior<,>),
                ];
            }
        );

        var appSettings = new AppSettings();

        builder.Configuration.Bind(appSettings);

        services.AddSingleton(appSettings);

        services.AddRateLimiting();

        services.AddVersioning();
        services.AddEndpoints(typeof(IChatApiMarker));

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IChatApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        builder.AddPersistenceServices();

        services.AddBackplaneServices();

        // Configure ClaimsPrincipal
        services.AddTransient(s => s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        builder.AddChatStreamingServices();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
