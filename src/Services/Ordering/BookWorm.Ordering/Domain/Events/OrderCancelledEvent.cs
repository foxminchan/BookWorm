namespace BookWorm.Ordering.Domain.Events;

public sealed class OrderCancelledEvent(Order order) : DomainEvent
{
    public Order Order { get; } = order;
}
