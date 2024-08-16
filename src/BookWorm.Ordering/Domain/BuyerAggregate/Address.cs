using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.Domain.BuyerAggregate;

public sealed class Address : ValueObject
{
    private Address() { }

    public Address(string? street, string? city, string? province)
    {
        Street = Guard.Against.NullOrEmpty(street);
        City = Guard.Against.NullOrEmpty(city);
        Province = Guard.Against.NullOrEmpty(province);
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
