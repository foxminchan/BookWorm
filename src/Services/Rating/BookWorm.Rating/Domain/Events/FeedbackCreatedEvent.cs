using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.Rating.Domain.Events;

public sealed class FeedbackCreatedEvent(Guid bookId, int rating, Guid feedbackId) : DomainEvent
{
    public Guid BookId { get; init; } = bookId;
    public int Rating { get; init; } = rating;
    public Guid FeedbackId { get; init; } = feedbackId;
}
