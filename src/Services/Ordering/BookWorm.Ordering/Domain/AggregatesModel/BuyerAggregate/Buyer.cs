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

    [DisallowNull]
    public string? Name { get; private set; }

    [DisallowNull]
    public Address? Address { get; private set; }

    [NotMapped]
    public string? FullAddress => Address?.ToString();

    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    /// <summary>
    ///     Updates the buyer's address with new street, city, and province information.
    /// </summary>
    /// <param name="street">The street address of the buyer.</param>
    /// <param name="city">The city where the buyer is located.</param>
    /// <param name="province">The province or state where the buyer is located.</param>
    /// <returns>The current <see cref="Buyer" /> instance with the updated address.</returns>
    public Buyer UpdateAddress(string street, string city, string province)
    {
        Address = new(street, city, province);
        return this;
    }
}
