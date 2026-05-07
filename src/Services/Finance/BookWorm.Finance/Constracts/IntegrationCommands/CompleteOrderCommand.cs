namespace BookWorm.Contracts;

public sealed record CompleteOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
