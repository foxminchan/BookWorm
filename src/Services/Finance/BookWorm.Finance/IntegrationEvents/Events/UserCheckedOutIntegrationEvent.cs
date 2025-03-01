namespace BookWorm.Contracts;

[AsyncApi]
[Channel($"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(UserCheckedOutIntegrationEvent)}")]
[PublishOperation(
    typeof(UserCheckedOutIntegrationEvent),
    OperationId = nameof(UserCheckedOutIntegrationEvent),
    Summary = "Basket deleted failed"
)]
public sealed record UserCheckedOutIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
