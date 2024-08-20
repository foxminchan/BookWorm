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

    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public Guid BookId { get; private set; }
    public Guid OrderId { get; private set; } = default!;
    public Order? Order { get; private set; } = default!;
}
