using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Scheduler, TopicName = "resend-error-email")]
[MessageIdentity("BookWorm.Contracts.ResendErrorEmailIntegrationEvent")]
public sealed record ResendErrorEmailIntegrationEvent : IntegrationEvent;
