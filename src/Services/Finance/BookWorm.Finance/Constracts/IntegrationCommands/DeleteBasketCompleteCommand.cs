namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Finance, TopicName = "delete-basket-complete")]
public sealed record DeleteBasketCompleteCommand(Guid OrderId, decimal TotalMoney)
    : IntegrationEvent;
