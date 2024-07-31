using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Basket.IntegrationEvents.Events;

public sealed class OrderCreatedIntegrationEvent(Guid orderId, Guid basketId) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
    public Guid BasketId { get; init; } = Guard.Against.Default(basketId);
}
