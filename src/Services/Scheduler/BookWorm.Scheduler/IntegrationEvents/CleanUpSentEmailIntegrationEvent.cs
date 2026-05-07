using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.CleanUpSentEmailIntegrationEvent")]
public sealed record CleanUpSentEmailIntegrationEvent : IntegrationEvent;
