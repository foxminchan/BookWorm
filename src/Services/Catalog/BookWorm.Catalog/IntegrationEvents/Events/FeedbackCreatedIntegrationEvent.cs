using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.FeedbackCreatedIntegrationEvent")]
public sealed record FeedbackCreatedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;
