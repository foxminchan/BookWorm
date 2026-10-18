using BookWorm.Constants.Aspire;
using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Scheduler, TopicName = "resend-error-email")]
[MessageIdentity("BookWorm.Contracts.ResendErrorEmailIntegrationEvent")]
public sealed record ResendErrorEmailIntegrationEvent : IntegrationEvent;
