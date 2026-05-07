using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.OrderStatusChangedToCancelIntegrationEvent")]
public sealed record OrderStatusChangedToCancelIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
