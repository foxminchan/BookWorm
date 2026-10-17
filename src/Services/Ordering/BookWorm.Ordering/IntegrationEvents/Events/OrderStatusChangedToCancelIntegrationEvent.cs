using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Ordering)]
[MessageIdentity("BookWorm.Contracts.OrderStatusChangedToCancelIntegrationEvent")]
public sealed record OrderStatusChangedToCancelIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
