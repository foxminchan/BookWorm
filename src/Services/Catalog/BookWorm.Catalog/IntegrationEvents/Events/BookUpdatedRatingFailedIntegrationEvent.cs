using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Catalog)]
[MessageIdentity("BookWorm.Contracts.BookUpdatedRatingFailedIntegrationEvent")]
public sealed record BookUpdatedRatingFailedIntegrationEvent(Guid FeedbackId) : IntegrationEvent;
