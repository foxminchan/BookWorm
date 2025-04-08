namespace BookWorm.Contracts;

public sealed record BasketDeletedCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    decimal TotalMoney
) : IntegrationEvent;
