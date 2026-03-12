using Wolverine.Persistence.Sagas;

namespace BookWorm.Contracts;

public sealed record OrderStatusChangedToCompleteIntegrationEvent(
    [property: SagaIdentity] Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;
