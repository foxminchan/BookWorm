namespace BookWorm.Ordering.Domain.OrderAggregate.Events;

public sealed class OrderCreatedEvent(Guid id) : EventBase
{
    public Guid Id { get; init; } = Guard.Against.Default(id);
    public Status Status { get; init; } = Status.Pending;
}
