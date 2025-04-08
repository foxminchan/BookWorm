namespace BookWorm.Contracts;

public sealed record PlaceOrderCommand(
    Guid BasketId,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;
