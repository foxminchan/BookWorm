namespace BookWorm.Contracts;

[AsyncApi]
[Channel($"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(BasketDeletedCompleteIntegrationEvent)}")]
[PublishOperation(
    typeof(BasketDeletedCompleteIntegrationEvent),
    OperationId = nameof(BasketDeletedCompleteIntegrationEvent),
    Summary = "Basket deleted complete"
)]
public sealed record BasketDeletedCompleteIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    decimal TotalMoney
) : IntegrationEvent;
