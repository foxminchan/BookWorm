using BookWorm.Basket.Features.Get;
using BookWorm.Chassis;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Constants.Core;
using BookWorm.SharedKernel;
using MassTransit;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Basket.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddDefaultAuthentication().WithKeycloakClaimsTransformation();

        services
            .AddAuthorizationBuilder()
            .SetDefaultPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole(Authorization.Roles.User)
                    .RequireScope(
                        $"{Services.Basket}_{Authorization.Actions.Read}",
                        $"{Services.Basket}_{Authorization.Actions.Write}"
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
                    typeof(IBasketApiMarker),
                ];

                options.PipelineBehaviors =
                [
                    typeof(ActivityBehavior<,>),
                    typeof(LoggingBehavior<,>),
                    typeof(ValidationBehavior<,>),
                    typeof(GetBasketPostProcessor),
                ];
            }
        );

        services.AddRateLimiting();

        // Add database configuration
        builder
            .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
            .WithDistributedCache(options => options.InstanceName = "ShoppingCarts");

        services.AddSingleton<IBasketRepository, BasketRepository>();

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IBasketApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IBasketApiMarker));

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure ClaimsPrincipal
        services.AddTransient(s => s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

        // Configure EventBus
        builder.AddEventBus(typeof(IBasketApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
