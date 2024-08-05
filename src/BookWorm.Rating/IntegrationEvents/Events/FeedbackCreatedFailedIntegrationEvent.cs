using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Rating.IntegrationEvents.Events;

public sealed class FeedbackCreatedFailedIntegrationEvent(string feedbackId) : IIntegrationEvent
{
    public string FeedbackId { get; init; } = Guard.Against.NullOrEmpty(feedbackId);
}
