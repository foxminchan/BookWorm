namespace BookWorm.Rating.Domain.Events;

public sealed class FeedbackRevertedEvent(Guid bookId) : DomainEvent
{
    public Guid BookId { get; init; } = bookId;
}
