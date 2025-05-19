using ZLinq;

namespace BookWorm.SharedKernel.SeedWork;

public abstract class ValueObject
{
    private static bool EqualOperator(ValueObject left, ValueObject? right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }

        return ReferenceEquals(left, null) || left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject? right)
    {
        return !EqualOperator(left, right);
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .AsValueEnumerable()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .AsValueEnumerable()
            .Select(x => x is not null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    public ValueObject? GetCopy()
    {
        return MemberwiseClone() as ValueObject;
    }
}
