namespace BookWorm.Contracts;

[AsyncApi]
[Channel($"{nameof(BookWorm)}.{nameof(Contracts)}:{nameof(BasketDeletedFailedIntegrationEvent)}")]
[PublishOperation(
    typeof(BasketDeletedFailedIntegrationEvent),
    OperationId = nameof(BasketDeletedFailedIntegrationEvent),
    Summary = "Basket deleted failed"
)]
public sealed record BasketDeletedFailedIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
