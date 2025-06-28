namespace BookWorm.Contracts;

public sealed record PlaceOrderTimeoutIntegrationEvent(Guid OrderId) : IntegrationEvent;
