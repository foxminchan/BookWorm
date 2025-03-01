namespace BookWorm.Contracts;

[AsyncApi]
[Channel(
    $"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(OrderStatusChangedToCancelIntegrationEvent)}"
)]
[PublishOperation(
    typeof(OrderStatusChangedToCancelIntegrationEvent),
    OperationId = nameof(OrderStatusChangedToCancelIntegrationEvent),
    Summary = "Order status changed to cancel"
)]
public sealed record OrderStatusChangedToCancelIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
