namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Finance, TopicName = "place-order-timeout")]
[MessageIdentity("BookWorm.Contracts.PlaceOrderTimeoutIntegrationEvent")]
public sealed record PlaceOrderTimeoutIntegrationEvent(Guid OrderId) : IntegrationEvent;
