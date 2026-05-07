using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.BasketDeletedCompleteIntegrationEvent")]
public sealed record BasketDeletedCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    decimal TotalMoney
) : IntegrationEvent;
