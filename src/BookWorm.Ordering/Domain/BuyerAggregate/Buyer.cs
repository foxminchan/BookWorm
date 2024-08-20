namespace BookWorm.Ordering.Domain.BuyerAggregate;

public sealed class Buyer : EntityBase, IAggregateRoot
{
    private readonly List<Order> _orders = [];

    private Buyer()
    {
        // EF Core
    }

    public Buyer(Guid id, string name, Address address)
    {
        Id = Guard.Against.Default(id);
        Name = Guard.Against.NullOrEmpty(name);
        Address = Guard.Against.Null(address);
    }

    public string? Name { get; private set; }
    public Address? Address { get; private set; }

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public void UpdateAddress(Address address)
    {
        Address = Guard.Against.Null(address);
    }
}
