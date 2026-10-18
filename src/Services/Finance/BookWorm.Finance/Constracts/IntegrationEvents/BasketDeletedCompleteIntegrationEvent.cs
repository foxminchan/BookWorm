using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Basket, TopicName = "delete-complete")]
[MessageIdentity("BookWorm.Contracts.BasketDeletedCompleteIntegrationEvent")]
public sealed record BasketDeletedCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    decimal TotalMoney
) : IntegrationEvent;
