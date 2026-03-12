using Wolverine.Persistence.Sagas;

namespace BookWorm.Contracts;

public sealed record BasketDeletedFailedIntegrationEvent(
    [property: SagaIdentity] Guid OrderId,
    Guid BasketId,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
