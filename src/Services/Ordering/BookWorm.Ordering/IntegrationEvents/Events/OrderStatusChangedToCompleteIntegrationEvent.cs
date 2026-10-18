using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Ordering, TopicName = "order-status-change-to-complete")]
[MessageIdentity("BookWorm.Contracts.OrderStatusChangedToCompleteIntegrationEvent")]
public sealed record OrderStatusChangedToCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
