using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Catalog.Extensions;

public static class DecimalValueExtensions
{
    /// <summary>
    /// Converts a .NET decimal to a protobuf Decimal message.
    /// Uses Google's protobuf decimal representation where nanos is always positive.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    /// <returns>A protobuf Decimal message.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is too large to represent.</exception>
    public static Grpc.Services.Decimal ToDecimal(decimal value)
    {
        // Check bounds - protobuf int64 has limits
        if (value > long.MaxValue || value < long.MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), 
                $"Value {value} is outside the range that can be represented in protobuf Decimal type.");
        }
        
        // For negative numbers, we need to use floor division
        // to ensure nanos is always positive
        var units = (long)Math.Floor(value);
        var fractionalPart = value - units;
        
        // Convert fractional part to nanoseconds (9 decimal places)
        // fractionalPart is always >= 0 after floor division
        var nanos = (int)Math.Round(fractionalPart * 1_000_000_000m);
        
        // Handle cases where rounding causes nanos to be 1 billion
        if (nanos >= 1_000_000_000)
        {
            units += 1;
            nanos = 0;
        }
        
        return new Grpc.Services.Decimal
        {
            Units = units,
            Nanos = nanos
        };
    }

    /// <summary>
    /// Converts a protobuf Decimal message to a .NET decimal.
    /// </summary>
    /// <param name="value">The protobuf Decimal to convert.</param>
    /// <returns>A .NET decimal value.</returns>
    public static decimal FromDecimal(Grpc.Services.Decimal value)
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
}