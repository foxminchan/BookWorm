using BookWorm.Core.SeedWork;

namespace BookWorm.Ordering.IntegrationEvents.Events;

public sealed class OrderCreatedIntegrationEvent(Guid orderId, Guid basketId, string? email) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
    public Guid BasketId { get; init; } = Guard.Against.Default(basketId);
    public string? Email { get; init; } = email;
}
