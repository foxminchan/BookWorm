using Wolverine.Persistence.Sagas;

namespace BookWorm.Contracts;

public sealed record PlaceOrderTimeoutIntegrationEvent([property: SagaIdentity] Guid OrderId)
    : IntegrationEvent;
