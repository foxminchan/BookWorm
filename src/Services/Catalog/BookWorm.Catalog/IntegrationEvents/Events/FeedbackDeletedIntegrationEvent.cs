using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Rating)]
[MessageIdentity("BookWorm.Contracts.FeedbackDeletedIntegrationEvent")]
public sealed record FeedbackDeletedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;
