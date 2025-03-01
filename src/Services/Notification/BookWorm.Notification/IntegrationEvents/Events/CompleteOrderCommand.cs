namespace BookWorm.Contracts;

public sealed record CompleteOrderCommand(Guid OrderId, string? Email, decimal TotalMoney)
    : IntegrationEvent;
