using Wolverine.Persistence.Sagas;

namespace BookWorm.Contracts;

public sealed record BasketDeletedCompleteIntegrationEvent(
    [property: SagaIdentity] Guid OrderId,
    Guid BasketId,
    decimal TotalMoney
) : IntegrationEvent;
