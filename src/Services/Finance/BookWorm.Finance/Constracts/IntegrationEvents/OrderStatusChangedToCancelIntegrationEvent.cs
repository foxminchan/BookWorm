using BookWorm.Constants.Aspire;
using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(
    ServiceName = Services.Ordering,
    TopicName = "order-status-change-to-cancel"
)]
[MessageIdentity("BookWorm.Contracts.OrderStatusChangedToCancelIntegrationEvent")]
public sealed record OrderStatusChangedToCancelIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
