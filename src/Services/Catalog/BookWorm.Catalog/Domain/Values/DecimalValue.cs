namespace BookWorm.Catalog.Grpc.Services;

public partial class DecimalValue
{
    private const decimal NanoFactor = 1_000_000_000;

    public DecimalValue(long units, int nanos)
    {
        Units = units;
        Nanos = nanos;
    }

    public static implicit operator decimal?(DecimalValue? grpcDecimal)
    {
        return grpcDecimal is null ? null : grpcDecimal.Units + (grpcDecimal.Nanos / NanoFactor);
    }

    public static implicit operator DecimalValue?(decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        var units = decimal.ToInt64(value.Value);
        var nanos = decimal.ToInt32((value.Value - units) * NanoFactor);
        return new(units, nanos);
    }
}
