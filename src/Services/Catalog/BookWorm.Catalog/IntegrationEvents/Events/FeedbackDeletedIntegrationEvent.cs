using BookWorm.Constants.Aspire;
using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Rating, TopicName = "feedback-delete")]
[MessageIdentity("BookWorm.Contracts.FeedbackDeletedIntegrationEvent")]
public sealed record FeedbackDeletedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;
