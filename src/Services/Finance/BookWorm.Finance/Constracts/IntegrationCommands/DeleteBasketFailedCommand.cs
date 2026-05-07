namespace BookWorm.Contracts;

public sealed record DeleteBasketFailedCommand(
    Guid BasketId,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;
