namespace BookWorm.Contracts;

[AsyncApi]
[Channel("basket-checkout-failed")]
[SubscribeOperation(
    typeof(DeleteBasketFailedCommand),
    OperationId = nameof(DeleteBasketFailedCommand),
    Summary = "Delete basket failed"
)]
public sealed record DeleteBasketFailedCommand(
    Guid BasketId,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;
