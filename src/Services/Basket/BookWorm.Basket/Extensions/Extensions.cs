using BookWorm.Constants.Aspire;
using BookWorm.Catalog.Grpc.Services;
using MassTransit;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Basket.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddDefaultCors();

        builder.AddDefaultOpenApi();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

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

        builder.AddDefaultAsyncApi([typeof(IBasketApiMarker)]);
    }

    /// <summary>
    /// Converts a protobuf Decimal message to a .NET decimal.
    /// </summary>
    /// <param name="value">The protobuf Decimal to convert.</param>
    /// <returns>A .NET decimal value.</returns>
    public static decimal ToDecimal(this Decimal value)
    {
        if (value is null)
        {
            return 0m;
        }
        
        // Convert nanos back to fractional part (always positive)
        var fractionalPart = (decimal)value.Nanos / 1_000_000_000m;
        
        // Combine units and fractional part
        return value.Units + fractionalPart;
    }

    /// <summary>
    /// Gets the price from BookResponse, preferring sale price if available.
    /// </summary>
    /// <param name="book">The book response.</param>
    /// <returns>The effective price as a decimal.</returns>
    public static decimal GetPrice(this BookResponse book)
    {
        return book.Price.ToDecimal();
    }

    /// <summary>
    /// Gets the sale price from BookResponse if available.
    /// </summary>
    /// <param name="book">The book response.</param>
    /// <returns>The sale price as a decimal, or null if not available.</returns>
    public static decimal? GetPriceSale(this BookResponse book)
    {
        // Check if PriceSale has meaningful content (non-zero units or nanos)
        if (book.PriceSale is not null && (book.PriceSale.Units != 0 || book.PriceSale.Nanos != 0))
        {
            return book.PriceSale.ToDecimal();
        }
        
        return null;
    }
}
