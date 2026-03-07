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

    /// <summary>
    ///     Marks the order as deleted by setting the IsDeleted flag to true.
    ///     This performs a soft delete operation, preserving the order data while indicating it should be treated as deleted.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
    }

    /// <summary>
    ///     Marks the order as completed if it is currently in a new state.
    /// </summary>
    /// <returns>The current order instance with its status updated to completed.</returns>
    /// <exception cref="OrderingDomainException">
    ///     Thrown when the order is not in <see cref="Status.New" /> status.
    /// </exception>
    /// <remarks>
    ///     This method only transitions orders from New status to Completed status.
    ///     When successful, this method raises an OrderCompletedEvent domain event.
    /// </remarks>
    public Order MarkAsCompleted()
    {
        if (Status != Status.New)
        {
            throw new OrderingDomainException(
                $"Cannot complete order in '{Status}' status; only '{Status.New}' orders can be completed."
            );
        }

        Status = Status.Completed;
        RegisterDomainEvent(new OrderCompletedEvent(this));
        return this;
    }

    /// <summary>
    ///     Marks the order as cancelled if it is in the New status.
    /// </summary>
    /// <returns>The current order instance with its status updated to cancelled.</returns>
    /// <exception cref="OrderingDomainException">
    ///     Thrown when the order is not in <see cref="Status.New" /> status.
    /// </exception>
    /// <remarks>
    ///     When successfully cancelled, it updates the status to Cancelled and raises an OrderCancelledEvent.
    /// </remarks>
    public Order MarkAsCanceled()
    {
        if (Status != Status.New)
        {
            throw new OrderingDomainException(
                $"Cannot cancel order in '{Status}' status; only '{Status.New}' orders can be cancelled."
            );
        }

        Status = Status.Cancelled;
        RegisterDomainEvent(new OrderCancelledEvent(this));
        return this;
    }
}
