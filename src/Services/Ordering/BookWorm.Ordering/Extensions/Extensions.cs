using System.Text.Json;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chassis.Utilities.Converters;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Features.Orders.Get;
using BookWorm.Ordering.Infrastructure.DistributedLock;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Ordering.Extensions;

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

        builder.AddDefaultOpenApi();

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
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ActivityBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped<CreateOrderPreProcessor>()
            .AddScoped<GetOrderPostProcessor>();

        services.AddSingleton(
            new JsonSerializerOptions { Converters = { DecimalJsonConverter.Instance } }
        );

        services.AddFeatureManagement();

        services.AddRateLimiting();

        builder.AddRedaction();

        builder.AddPersistenceServices();

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IOrderingApiMarker>(
            includeInternalTypes: true
        );

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        services.AddScoped<IEventMapper, EventMapper>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IRequestManager, RequestManager>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IOrderingApiMarker));

        // Add event bus configuration
        builder.AddEventBus(
            typeof(IOrderingApiMarker),
            cfg =>
            {
                cfg.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
                {
                    o.QueryDelay = TimeSpan.FromSeconds(1);

                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);

                    o.UsePostgres();

                    o.UseBusOutbox();
                });

                cfg.AddConfigureEndpointsCallback(
                    (context, _, configurator) =>
                        configurator.UseEntityFrameworkOutbox<OrderingDbContext>(context)
                );
            }
        );

        // Configure ClaimsPrincipal
        services.AddTransient(s => s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User);

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure Redis distributed lock
        builder.AddDistributedLock();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
