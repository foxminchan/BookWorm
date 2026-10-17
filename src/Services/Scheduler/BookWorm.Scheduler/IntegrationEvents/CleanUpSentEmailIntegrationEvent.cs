using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Scheduler)]
[MessageIdentity("BookWorm.Contracts.CleanUpSentEmailIntegrationEvent")]
public sealed record CleanUpSentEmailIntegrationEvent : IntegrationEvent;
