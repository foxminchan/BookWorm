namespace BookWorm.Contracts;

public sealed record DeleteBasketCompleteCommand(Guid OrderId, decimal TotalMoney)
    : IntegrationEvent;
