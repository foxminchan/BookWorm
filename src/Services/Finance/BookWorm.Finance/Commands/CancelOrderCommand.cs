namespace BookWorm.Contracts;

[AsyncApi]
[Channel("notification-cancel-order")]
[SubscribeOperation(
    typeof(CancelOrderCommand),
    OperationId = nameof(CancelOrderCommand),
    Summary = "Cancel order notification"
)]
public sealed record CancelOrderCommand(Guid OrderId, string? Email, decimal TotalMoney)
    : IntegrationEvent;
