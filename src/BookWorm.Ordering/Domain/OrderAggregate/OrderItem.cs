using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.Domain.OrderAggregate;

public sealed class OrderItem : EntityBase
{
    private OrderItem()
    {
        // EF Core
    }

    public OrderItem(Guid bookId, int quantity, decimal price)
    {
        BookId = Guard.Against.Default(bookId);
        Quantity = Guard.Against.NegativeOrZero(quantity);
        Price = Guard.Against.NegativeOrZero(price);
    }

    public int Quantity { get; }
    public decimal Price { get; }
    public Guid BookId { get; private set; }
    public Guid OrderId { get; }
    public Order? Order { get; private set; } = default!;

    public decimal TotalPrice => Quantity * Price;
}
