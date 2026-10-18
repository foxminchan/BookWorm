using BookWorm.Constants.Aspire;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Finance, TopicName = "delete-basket-complete")]
public sealed record DeleteBasketCompleteCommand(Guid OrderId, decimal TotalMoney)
    : IntegrationEvent;
