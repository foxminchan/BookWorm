namespace BookWorm.Contracts;

[AsyncApi]
[Channel("basket-checkout-complete")]
[SubscribeOperation(
    typeof(DeleteBasketCompleteCommand),
    OperationId = nameof(DeleteBasketCompleteCommand),
    Summary = "Delete basket complete"
)]
public sealed record DeleteBasketCompleteCommand(Guid OrderId, decimal TotalMoney)
    : IntegrationEvent;
