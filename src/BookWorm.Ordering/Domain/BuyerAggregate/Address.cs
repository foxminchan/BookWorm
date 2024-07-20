using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.Domain.BuyerAggregate;

public sealed class Address : ValueObject
{
    private Address() { }

    public Address(string street, string city, string province)
    {
        Street = street;
        City = city;
        Province = province;
    }

    public string? Street { get; }
    public string? City { get; }
    public string? Province { get; }

    public override string ToString()
    {
        return $"{Street}, {City}, {Province}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ToString();
    }
}
