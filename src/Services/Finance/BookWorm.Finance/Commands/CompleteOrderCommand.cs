namespace BookWorm.Contracts;

[AsyncApi]
[Channel("notification-complete-order")]
[SubscribeOperation(
    typeof(CompleteOrderCommand),
    OperationId = nameof(CompleteOrderCommand),
    Summary = "Complete order notification"
)]
public sealed record CompleteOrderCommand(Guid OrderId, string? Email, decimal TotalMoney)
    : IntegrationEvent;
