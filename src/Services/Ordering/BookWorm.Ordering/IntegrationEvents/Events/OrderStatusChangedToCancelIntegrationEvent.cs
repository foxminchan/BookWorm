namespace BookWorm.Contracts;

public sealed record OrderStatusChangedToCancelIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
