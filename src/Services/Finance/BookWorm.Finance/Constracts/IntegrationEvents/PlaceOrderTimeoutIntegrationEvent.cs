using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Finance)]
[MessageIdentity("BookWorm.Contracts.PlaceOrderTimeoutIntegrationEvent")]
public sealed record PlaceOrderTimeoutIntegrationEvent(Guid OrderId) : IntegrationEvent;
