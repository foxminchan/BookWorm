using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Rating, TopicName = "feedback-create")]
[MessageIdentity("BookWorm.Contracts.FeedbackCreatedIntegrationEvent")]
public sealed record FeedbackCreatedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;
