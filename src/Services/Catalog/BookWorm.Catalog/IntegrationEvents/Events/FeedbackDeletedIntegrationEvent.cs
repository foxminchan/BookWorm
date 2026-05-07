using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.FeedbackDeletedIntegrationEvent")]
public sealed record FeedbackDeletedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;
