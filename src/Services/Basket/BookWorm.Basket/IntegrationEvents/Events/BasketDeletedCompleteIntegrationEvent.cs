using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Basket)]
[MessageIdentity("BookWorm.Contracts.BasketDeletedCompleteIntegrationEvent")]
public sealed record BasketDeletedCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    decimal TotalMoney
) : IntegrationEvent;
