namespace BookWorm.Contracts;

public sealed record CancelOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
