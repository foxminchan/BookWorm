using BookWorm.Constants.Aspire;
using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Catalog, TopicName = "book-update-rating-failed")]
[MessageIdentity("BookWorm.Contracts.BookUpdatedRatingFailedIntegrationEvent")]
public sealed record BookUpdatedRatingFailedIntegrationEvent(Guid FeedbackId) : IntegrationEvent;
