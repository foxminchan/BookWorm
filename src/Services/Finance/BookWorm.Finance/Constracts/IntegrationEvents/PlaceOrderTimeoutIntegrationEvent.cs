using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.PlaceOrderTimeoutIntegrationEvent")]
public sealed record PlaceOrderTimeoutIntegrationEvent(Guid OrderId) : IntegrationEvent;
