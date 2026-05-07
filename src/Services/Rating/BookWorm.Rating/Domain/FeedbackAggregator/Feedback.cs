using BookWorm.Rating.Domain.Events;
using BookWorm.Rating.Domain.Exceptions;

namespace BookWorm.Rating.Domain.FeedbackAggregator;

public sealed class Feedback() : AuditableEntity, IAggregateRoot
{
    public Feedback(Guid bookId, string? firstName, string? lastName, string? comment, int rating)
        : this()
    {
        // Generate the Id client-side (UUID v7) so the FeedbackCreatedEvent below carries
        // the real identifier. The EF configuration falls back to the Postgres `uuidv7()`
        // default only when Id is Guid.Empty, so this assignment is honoured on insert.
        Id = Guid.CreateVersion7();
        BookId = bookId;
        FirstName = firstName;
        LastName = lastName;
        Comment = comment;
        Rating = rating is < 0 or > 5
            ? throw new RatingDomainException("Rating must be between 0 and 5.")
            : rating;
        RegisterDomainEvent(new FeedbackCreatedEvent(BookId, Rating, Id));
    }

    public Guid BookId { get; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Comment { get; private set; }
    public int Rating { get; }

    /// <summary>
    ///     Marks the feedback for removal and raises a domain event to notify that the feedback has been deleted.
    /// </summary>
    /// <returns>The current instance of <see cref="Feedback" /> after registering the deletion event.</returns>
    public Feedback Remove()
    {
        RegisterDomainEvent(new FeedbackDeletedEvent(BookId, Rating, Id));
        return this;
    }
}
