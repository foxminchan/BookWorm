using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.IntegrationEvents.Events;

public sealed class OrderCancelledIntegrationEvent(Guid orderId) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
}
