namespace BookWorm.Contracts;

public sealed class FeedbackCreatedFailedIntegrationEvent(string feedbackId) : IIntegrationEvent
{
    public string FeedbackId { get; init; } = Guard.Against.NullOrEmpty(feedbackId);
}
