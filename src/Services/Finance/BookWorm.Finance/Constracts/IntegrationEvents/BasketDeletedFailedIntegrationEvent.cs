using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Basket)]
[MessageIdentity("BookWorm.Contracts.BasketDeletedFailedIntegrationEvent")]
public sealed record BasketDeletedFailedIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
