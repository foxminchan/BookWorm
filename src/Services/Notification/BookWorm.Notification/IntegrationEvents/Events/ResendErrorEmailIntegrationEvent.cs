using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Scheduler)]
[MessageIdentity("BookWorm.Contracts.ResendErrorEmailIntegrationEvent")]
public sealed record ResendErrorEmailIntegrationEvent : IntegrationEvent;
