using BookWorm.Rating.Domain.Events;
using BookWorm.Rating.Domain.Exceptions;

namespace BookWorm.Rating.Domain.FeedbackAggregator;

public sealed class Feedback() : AuditableEntity, IAggregateRoot
{
    public Feedback(Guid bookId, string? firstName, string? lastName, string? comment, int rating)
        : this()
    {
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

    public void Remove()
    {
        RegisterDomainEvent(new FeedbackDeletedEvent(BookId, Rating, Id));
    }
}
