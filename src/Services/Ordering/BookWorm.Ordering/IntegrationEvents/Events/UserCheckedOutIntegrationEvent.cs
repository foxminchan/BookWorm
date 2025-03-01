namespace BookWorm.Contracts;

public sealed record UserCheckedOutIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
