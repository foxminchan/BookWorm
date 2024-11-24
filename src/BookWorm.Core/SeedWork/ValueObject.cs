namespace BookWorm.Core.SeedWork;

[Serializable]
public abstract class ValueObject : IEquatable<ValueObject>
{
    public virtual bool Equals(ValueObject? other)
    {
        return other is not null && ValuesAreEqual(other);
    }

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        return obj is ValueObject valueObject && ValuesAreEqual(valueObject);
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject? a, ValueObject? b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                default(int),
                (hashcode, value) => HashCode.Combine(hashcode, value.GetHashCode())
            );
    }

    private bool ValuesAreEqual(ValueObject valueObject)
    {
        return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }
}
