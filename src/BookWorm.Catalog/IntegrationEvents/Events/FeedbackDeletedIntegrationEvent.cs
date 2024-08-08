using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Catalog.IntegrationEvents.Events;

public sealed class FeedbackDeletedIntegrationEvent(string feedbackId, Guid bookId, int rating) : IIntegrationEvent
{
    public string FeedbackId { get; init; } = Guard.Against.NullOrEmpty(feedbackId);
    public Guid BookId { get; init; } = Guard.Against.Default(bookId);
    public int Rating { get; init; } = Guard.Against.OutOfRange(rating, nameof(rating), 0, 5);
}
