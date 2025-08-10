using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.CQRS.Mediator;
using BookWorm.Chassis.CQRS.Pipelines;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.OpenTelemetry.ActivityScope;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Features.Orders.Get;
using BookWorm.Ordering.Infrastructure.DistributedLock;

namespace BookWorm.Ordering.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        services.AddFeatureManagement();

        services.AddRateLimiting();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        builder.AddDefaultOpenApi();

        builder.AddRedaction();

        services.AddSignalR().AddNamedAzureSignalR(Components.Azure.SignalR);

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        builder.AddPersistenceServices();

        // Configure MediatR
        services.AddMediatR<IOrderingApiMarker>(configuration =>
        {
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddRequestPreProcessor<CreateOrderPreProcessor>();
            configuration.AddRequestPostProcessor<GetOrderPostProcessor>();
        });

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
        services.AddTransient<ClaimsPrincipal>(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User
        );

        // Configure gRPC
        builder.AddGrpcServices();

        // Configure Redis distributed lock
        builder.AddDistributedLock();

        services.AddScoped<KeycloakTokenIntrospectionMiddleware>();
    }
}
