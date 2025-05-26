using StackExchange.Redis;

namespace BookWorm.Ordering.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        services.AddFeatureManagement();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        builder.AddPersistenceServices();

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IOrderingApiMarker>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
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

                    o.UsePostgres();

                    o.UseBusOutbox();
                });
            }
        );

        // Configure ClaimsPrincipal
        services.AddTransient<ClaimsPrincipal>(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User
        );

        // Configure gRPC
        services.AddGrpcServices();

        // Configure Redis distributed lock
        services.AddSingleton<IDistributedLockProvider>(
            _ => new RedisDistributedSynchronizationProvider(
                services
                    .BuildServiceProvider()
                    .GetRequiredService<IConnectionMultiplexer>()
                    .GetDatabase()
            )
        );

        builder.AddDefaultAsyncApi([typeof(IOrderingApiMarker)]);
    }
}
