namespace BookWorm.Ordering.Domain.OrderAggregate.Events;

public sealed class OrderCompletedEvent(Guid id) : EventBase
{
    public Guid Id { get; init; } = Guard.Against.Default(id);
    public Status Status { get; init; } = Status.Completed;
}
