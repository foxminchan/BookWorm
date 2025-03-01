namespace BookWorm.Ordering.Domain.Events;

public sealed class OrderCompletedEvent(Order order) : DomainEvent
{
    public Order Order { get; } = order;
}
