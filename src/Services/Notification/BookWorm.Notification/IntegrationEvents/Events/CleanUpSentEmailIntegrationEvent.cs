using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Scheduler, TopicName = "clean-up-sent-email")]
[MessageIdentity("BookWorm.Contracts.CleanUpSentEmailIntegrationEvent")]
public sealed record CleanUpSentEmailIntegrationEvent : IntegrationEvent;
