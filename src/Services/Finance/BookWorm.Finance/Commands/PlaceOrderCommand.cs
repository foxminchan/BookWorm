namespace BookWorm.Contracts;

[AsyncApi]
[Channel("basket-place-order")]
[SubscribeOperation(
    typeof(PlaceOrderCommand),
    OperationId = nameof(PlaceOrderCommand),
    Summary = "Delete a basket"
)]
public sealed record PlaceOrderCommand(
    Guid BasketId,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;
