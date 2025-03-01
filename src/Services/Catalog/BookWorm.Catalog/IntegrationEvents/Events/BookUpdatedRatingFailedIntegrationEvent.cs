namespace BookWorm.Contracts;

public sealed record BookUpdatedRatingFailedIntegrationEvent(Guid FeedbackId) : IntegrationEvent;
