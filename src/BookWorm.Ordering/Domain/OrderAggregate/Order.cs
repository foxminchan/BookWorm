using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;
using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.Domain.OrderAggregate;

public sealed class Order : EntityBase, IAggregateRoot
{
    private readonly List<OrderItem> _orderItems = [];

    private Order()
    {
        // EF Core
    }

    public Order(Guid buyerId, string? note)
    {
        BuyerId = Guard.Against.Default(buyerId);
        Note = note;
        Status = Status.Pending;
    }

    public string? Note { get; private set; }
    public Guid BuyerId { get; private set; }
    public Status Status { get; private set; }
    public Buyer? Buyer { get; private set; } = default!;

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public decimal TotalPrice => OrderItems.Sum(oi => oi.Price * oi.Quantity);

    public void AddOrderItem(Guid bookId, decimal price, int quantity)
    {
        _orderItems.Add(new(bookId, quantity, price));
    }

    public void MarkAsCompleted()
    {
        Status = Status.Completed;
    }

    public void MarkAsCanceled()
    {
        Status = Status.Canceled;
    }
}
