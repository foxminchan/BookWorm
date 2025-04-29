namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public sealed class OrderItem() : Entity
{
    public OrderItem(Guid bookId, int quantity, decimal price)
        : this()
    {
        BookId = bookId;
        Quantity =
            quantity > 0
                ? quantity
                : throw new OrderingDomainException("Quantity must be greater than zero");
        Price =
            price >= 0
                ? price
                : throw new OrderingDomainException("Price must be greater than or equal to zero");
    }

    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public Guid BookId { get; private set; }
    public Guid OrderId { get; private set; }
    public Order? Order { get; private set; } = null!;
}
