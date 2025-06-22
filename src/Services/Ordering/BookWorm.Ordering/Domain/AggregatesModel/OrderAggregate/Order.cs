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
    public Buyer? Buyer { get; private set; } = null!;

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    [NotMapped]
    public decimal TotalPrice => OrderItems.Select(item => item.GetTotalPrice()).Sum();

    public bool IsDeleted { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void MarkAsCompleted()
    {
        if (Status != Status.New)
        {
            return;
        }

        Status = Status.Completed;
        RegisterDomainEvent(new OrderCompletedEvent(this));
    }

    public void MarkAsCanceled()
    {
        if (Status != Status.New)
        {
            return;
        }

        Status = Status.Cancelled;
        RegisterDomainEvent(new OrderCancelledEvent(this));
    }
}
