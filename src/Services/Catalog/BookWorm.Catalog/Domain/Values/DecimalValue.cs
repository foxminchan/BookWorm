namespace BookWorm.Catalog.Grpc.Services;

[ExcludeFromCodeCoverage]
public sealed partial class DecimalValue
{
    private const decimal NanoFactor = 1_000_000_000;

    public DecimalValue(long units, int nanos)
    {
        Units = units;
        Nanos = nanos;
    }

    public static implicit operator decimal?(DecimalValue? value)
    {
        return value is null ? null : value.Units + (value.Nanos / NanoFactor);
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
