namespace BookWorm.Contracts;

public sealed class OrderCancelledIntegrationEvent(Guid orderId, string? email) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
    public string? Email { get; init; } = email;
}
