namespace BookWorm.SharedKernel.SeedWork;

public abstract class ValueObject
{
    private static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
    {
        return !EqualOperator(left, right);
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other || other.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    public ValueObject? GetCopy()
    {
        return MemberwiseClone() as ValueObject;
    }
}
