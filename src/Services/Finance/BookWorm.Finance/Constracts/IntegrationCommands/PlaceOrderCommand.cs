namespace BookWorm.Contracts;

public sealed record PlaceOrderCommand(
    Guid BasketId,
    string? FullName,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;
