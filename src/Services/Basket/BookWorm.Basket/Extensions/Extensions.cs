using BookWorm.Basket.Configurations;
using BookWorm.Basket.Features.Get;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.Constants.Core;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
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

        builder.AddAppSettings<BasketAppSettings>();

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

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Configure Mediator
        services
            .AddMediator(
                (MediatorOptions options) => options.ServiceLifetime = ServiceLifetime.Scoped
            )
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ActivityBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped<GetBasketPostProcessor>();

        builder.AddRateLimiting();

        // Add database configuration
        builder
            .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
            .WithAzureAuthentication();

        services.AddSingleton<IBasketRepository, BasketRepository>();

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IBasketApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IBasketApiMarker));
        services.AddDefaultOpenApi(options =>
            options.AddDocumentTransformer<OpenApiInfoDefinitionsTransformer<BasketAppSettings>>()
        );

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure ClaimsPrincipal
        services.AddTransient(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new ClaimsPrincipal()
        );

        // Configure EventBus
        builder.AddEventBus(typeof(IBasketApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
