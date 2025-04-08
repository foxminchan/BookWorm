namespace BookWorm.Contracts;

public sealed record BasketDeletedFailedIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
