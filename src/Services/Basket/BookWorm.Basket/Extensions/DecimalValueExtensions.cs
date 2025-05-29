using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Basket.Extensions;

public static class DecimalValueExtensions
{
    /// <summary>
    /// Converts a protobuf Decimal message to a .NET decimal.
    /// </summary>
    /// <param name="value">The protobuf Decimal to convert.</param>
    /// <returns>A .NET decimal value.</returns>
    public static decimal ToDecimal(this BookWorm.Catalog.Grpc.Services.Decimal value)
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