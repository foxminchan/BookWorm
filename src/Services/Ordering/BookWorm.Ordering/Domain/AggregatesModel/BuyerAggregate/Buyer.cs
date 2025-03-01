using System.ComponentModel.DataAnnotations.Schema;

namespace BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;

public sealed class Buyer() : Entity, IAggregateRoot
{
    private readonly List<Order> _orders = [];

    public Buyer(Guid id, string name, string street, string city, string province)
        : this()
    {
        Id = id;
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new OrderingDomainException("Name is required");
        Address = new(street, city, province);
    }

    public string? Name { get; private set; }
    public Address? Address { get; private set; }

    [NotMapped]
    public string? FullAddress => Address?.ToString();

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public void UpdateAddress(string street, string city, string province)
    {
        Address = new(street, city, province);
    }
}
