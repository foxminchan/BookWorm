namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Finance)]
public sealed record DeleteBasketCompleteCommand(Guid OrderId, decimal TotalMoney)
    : IntegrationEvent;
