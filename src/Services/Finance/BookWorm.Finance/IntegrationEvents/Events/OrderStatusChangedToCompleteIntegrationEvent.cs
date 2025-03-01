namespace BookWorm.Contracts;

[AsyncApi]
[Channel(
    $"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(OrderStatusChangedToCompleteIntegrationEvent)}"
)]
[PublishOperation(
    typeof(OrderStatusChangedToCompleteIntegrationEvent),
    OperationId = nameof(OrderStatusChangedToCompleteIntegrationEvent),
    Summary = "Order status changed to cancel"
)]
public sealed record OrderStatusChangedToCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
