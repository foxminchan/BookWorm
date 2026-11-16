using BookWorm.Basket.Configurations;
using BookWorm.Basket.Features.Get;
using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.OpenTelemetry;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Configurations;
using BookWorm.ServiceDefaults.ApiSpecification.OpenApi.Transformers;
using BookWorm.ServiceDefaults.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace BookWorm.Basket.Extensions;

internal static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationServices()
        {
            var services = builder.Services;

            builder.AddDefaultCors();

            builder.AddAppSettings<BasketAppSettings>();

            builder.AddDefaultAuthentication();

            services
                .AddAuthorizationBuilder()
                .SetDefaultPolicy(
                    new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole(Authorization.Roles.Customer)
                        .RequireScope(
                            $"{Services.Basket}_{Authorization.Actions.Read}",
                            $"{Services.Basket}_{Authorization.Actions.Write}"
                        )
                        .Build()
                );

            // Add exception handlers
            services.AddValidationExceptionHandler();
            services.AddNotFoundExceptionHandler();
            services.AddGlobalExceptionHandler();
            services.AddProblemDetails();

            // Configure Mediator
            services
                .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
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
            services.AddValidatorsFromAssemblyContaining<BasketApiMarker>(
                includeInternalTypes: true
            );

            services.AddActivityScope().AddCommandHandlerMetrics().AddQueryHandlerMetrics();

            // Configure endpoints
            services.AddVersioning();
            services.AddEndpoints(typeof(BasketApiMarker));
            services.AddDefaultOpenApi(options =>
                options
                    .MapVogenTypesInBasketApiMarker()
                    .ApplyOpenApiInfoDefinitions<BasketAppSettings>()
            );

            // Configure gRPC
            builder.AddGrpcServices();

            // Configure ClaimsPrincipal
            services.AddTransient(s =>
                s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User
                ?? new ClaimsPrincipal()
            );

            // Configure EventBus
            builder.AddEventBus<BasketApiMarker>(opts =>
                // Preserve per-order saga ordering on Kafka by routing all events that
                // expose an OrderId to the same partition.
                opts.MessagePartitioning.ByPropertyNamed("OrderId")
            );
        }
    }
}
