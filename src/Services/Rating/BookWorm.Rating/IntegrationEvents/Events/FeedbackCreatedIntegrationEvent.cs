using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Rating)]
[MessageIdentity("BookWorm.Contracts.FeedbackCreatedIntegrationEvent")]
public sealed record FeedbackCreatedIntegrationEvent(Guid BookId, int Rating, Guid FeedbackId)
    : IntegrationEvent;
