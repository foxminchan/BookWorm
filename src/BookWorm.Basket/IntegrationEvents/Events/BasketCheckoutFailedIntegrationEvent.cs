using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Basket.IntegrationEvents.Events;

public sealed class BasketCheckoutFailedIntegrationEvent(Guid orderId) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
}
