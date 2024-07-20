using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;
using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.Domain.BuyerAggregate;

public sealed class Buyer : EntityBase, IAggregateRoot
{
    private readonly List<Order> _orders = [];

    private Buyer()
    {
        // EF Core
    }

    public Buyer(string name, Address address)
    {
        Name = Guard.Against.NullOrEmpty(name);
        Address = Guard.Against.Null(address);
    }

    public string? Name { get; private set; }
    public Address? Address { get; private set; }

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
}
