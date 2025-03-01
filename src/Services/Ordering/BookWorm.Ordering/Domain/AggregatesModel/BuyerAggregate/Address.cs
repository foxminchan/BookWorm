namespace BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;

public sealed class Address() : ValueObject
{
    public Address(string? street, string? city, string? province)
        : this()
    {
        Street = !string.IsNullOrWhiteSpace(street)
            ? street
            : throw new OrderingDomainException("Street is required");
        City = !string.IsNullOrWhiteSpace(city)
            ? city
            : throw new OrderingDomainException("City is required");
        Province = !string.IsNullOrWhiteSpace(province)
            ? province
            : throw new OrderingDomainException("Province is required");
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
