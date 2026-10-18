using BookWorm.Constants.Aspire;
using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Basket, TopicName = "delete-failed")]
[MessageIdentity("BookWorm.Contracts.BasketDeletedFailedIntegrationEvent")]
public sealed record BasketDeletedFailedIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
