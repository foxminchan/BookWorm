using BookWorm.Basket.Configurations;
using BookWorm.Basket.Features.Get;
using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.OpenTelemetry;
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
            )
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
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
            .ApplyLoggingBehavior()
            .ApplyActivityBehavior()
            .ApplyValidationBehavior()
            .AddScoped<GetBasketPostProcessor>();

        builder.AddRateLimiting();

        // Add database configuration
        builder
            .AddRedisClientBuilder(Components.Redis, o => o.DisableAutoActivation = false)
            .WithAzureAuthentication();

        services.AddSingleton<IBasketRepository, BasketRepository>();

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IBasketApiMarker>(includeInternalTypes: true);

        services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

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

        services.AddKeycloakTokenIntrospection();
    }
}
