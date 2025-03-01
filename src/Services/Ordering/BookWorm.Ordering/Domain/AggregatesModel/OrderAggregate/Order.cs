using System.ComponentModel.DataAnnotations.Schema;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public sealed class Order() : AuditableEntity, IAggregateRoot, ISoftDelete
{
    private readonly List<OrderItem> _orderItems = [];

    public Order(Guid buyerId, string? note, List<OrderItem> orderItems)
        : this()
    {
        BuyerId = buyerId;
        Note = note;
        Status = Status.New;
        _orderItems = orderItems;
        RegisterDomainEvent(new OrderPlacedEvent(this));
    }

    public Status Status { get; private set; }
    public string? Note { get; private set; }
    public Guid BuyerId { get; private set; }
    public Buyer? Buyer { get; private set; } = default!;

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    [NotMapped]
    public decimal TotalPrice => OrderItems.Sum(oi => oi.Price * oi.Quantity);

    public bool IsDeleted { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void MarkAsCompleted()
    {
        Status = Status.Completed;
        RegisterDomainEvent(new OrderCompletedEvent(this));
    }

    public void MarkAsCanceled()
    {
        Status = Status.Cancelled;
        RegisterDomainEvent(new OrderCancelledEvent(this));
    }
}
