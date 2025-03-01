using BookWorm.ServiceDefaults.Keycloak;
using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddDefaultCors();

        services.AddFeatureManagement();

        services.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        // Add Redis configuration
        builder.AddRedisClient(Components.Redis);
        services.AddScoped<IRequestManager, RequestManager>();

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        services.AddDbContext<OrderingDbContext>(options =>
        {
            options
                .UseNpgsql(builder.Configuration.GetConnectionString(Components.Database.Ordering))
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                );
        });
        builder.EnrichNpgsqlDbContext<OrderingDbContext>();

        services.AddMigration<OrderingDbContext>();

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

        // Configure repositories
        services.AddScoped<IBuyerRepository, BuyerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

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

        // Configure EventStore
        builder.AddEventStore(options =>
        {
            options.Projections.LiveStreamAggregation<OrderSummary>();
            options.Projections.Add<Projection>(ProjectionLifecycle.Async);
        });

        services.AddAsyncApiDocs([typeof(IOrderingApiMarker)], nameof(Ordering));
    }

    private static void AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();

        services
            .AddGrpcClient<BookGrpcServiceClient>(o =>
            {
                o.Address = new("http+https://bookworm-catalog");
            })
            .AddStandardResilienceHandler();

        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcClient<BasketGrpcServiceClient>(o =>
            {
                o.Address = new("http+https://bookworm-basket");
            })
            .AddAuthToken()
            .AddStandardResilienceHandler();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
