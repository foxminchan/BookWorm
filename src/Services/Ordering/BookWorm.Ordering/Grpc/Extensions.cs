using BookWorm.Constants.Aspire;
using BookWorm.Catalog.Grpc.Services;
using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();

        services.AddGrpcServiceReference<BookGrpcServiceClient>(
            $"https://{Application.Catalog}",
            HealthStatus.Degraded
        );
        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcServiceReference<BasketGrpcServiceClient>(
                $"https://{Application.Basket}",
                HealthStatus.Degraded
            )
            .AddAuthToken();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
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
    public static decimal GetEffectivePrice(this BookResponse book)
    {
        // Check if PriceSale has meaningful content (non-zero units or nanos)
        if (book.PriceSale is not null && (book.PriceSale.Units != 0 || book.PriceSale.Nanos != 0))
        {
            return book.PriceSale.ToDecimal();
        }
        
        return book.Price.ToDecimal();
    }
}
