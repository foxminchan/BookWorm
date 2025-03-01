namespace BookWorm.Ordering.Domain.Events;

public sealed class OrderPlacedEvent(Order order) : DomainEvent
{
    public Order Order { get; } = order;
}
