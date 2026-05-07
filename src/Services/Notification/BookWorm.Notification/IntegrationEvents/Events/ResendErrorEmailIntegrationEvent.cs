using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.ResendErrorEmailIntegrationEvent")]
public sealed record ResendErrorEmailIntegrationEvent : IntegrationEvent;
