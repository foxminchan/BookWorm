namespace BookWorm.Contracts;

public sealed class BasketCheckoutFailedIntegrationEvent(Guid orderId) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
}
