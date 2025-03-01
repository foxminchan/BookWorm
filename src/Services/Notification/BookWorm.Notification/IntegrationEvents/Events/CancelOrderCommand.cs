namespace BookWorm.Contracts;

public sealed record CancelOrderCommand(Guid OrderId, string? Email, decimal TotalMoney)
    : IntegrationEvent;
