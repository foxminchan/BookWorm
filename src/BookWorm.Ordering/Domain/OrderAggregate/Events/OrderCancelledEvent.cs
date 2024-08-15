using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.Domain.OrderAggregate.Events;

public sealed class OrderCancelledEvent(Guid id) : EventBase
{
    public Guid Id { get; init; } = Guard.Against.Default(id);
    public Status Status { get; init; } = Status.Canceled;
}
