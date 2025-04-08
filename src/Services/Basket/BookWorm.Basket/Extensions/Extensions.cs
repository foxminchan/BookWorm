using MassTransit;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Basket.Extensions;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddEndpoints(typeof(IBasketApiMarker));

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        builder.AddRedisClient(Components.Redis);
        services.AddSingleton<IBasketRepository, BasketRepository>();

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IBasketApiMarker>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<IBasketApiMarker>(includeInternalTypes: true);

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        // Configure endpoints
        services.AddVersioning();
        services.AddEndpoints(typeof(IBasketApiMarker));

        // Configure gRPC
        services.AddGrpc();
        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"https://{Application.Catalog}",
            HealthStatus.Degraded
        );
        services.AddSingleton<IBookService, BookService>();

        // Configure ClaimsPrincipal
        services.AddTransient<ClaimsPrincipal>(s =>
            s.GetRequiredService<IHttpContextAccessor>().HttpContext!.User
        );

        // Configure EventBus
        builder.AddEventBus(typeof(IBasketApiMarker), cfg => cfg.AddInMemoryInboxOutbox());

        builder.AddAsyncApiDocs([typeof(IBasketApiMarker)]);
    }
}
