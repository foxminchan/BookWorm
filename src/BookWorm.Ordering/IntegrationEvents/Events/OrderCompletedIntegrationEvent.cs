using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.IntegrationEvents.Events;

public sealed class OrderCompletedIntegrationEvent(Guid orderId) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
}
