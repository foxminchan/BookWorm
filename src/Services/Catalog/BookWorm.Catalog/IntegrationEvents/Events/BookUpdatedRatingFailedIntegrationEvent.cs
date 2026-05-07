using Wolverine.Attributes;

namespace BookWorm.Contracts;

[MessageIdentity("BookWorm.Contracts.BookUpdatedRatingFailedIntegrationEvent")]
public sealed record BookUpdatedRatingFailedIntegrationEvent(Guid FeedbackId) : IntegrationEvent;
