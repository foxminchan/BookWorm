namespace BookWorm.Contracts;

public sealed record OrderStatusChangedToCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
