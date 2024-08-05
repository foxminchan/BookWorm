using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;
using MongoDB.Bson;

namespace BookWorm.Rating.Domain;

public sealed class Feedback : IAggregateRoot
{
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
    public Guid BookId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }

    public Feedback(Guid bookId, int rating, string? comment)
    {
        BookId = Guard.Against.Default(bookId);
        Rating = Guard.Against.OutOfRange(rating, nameof(rating), 0, 5);
        Comment = comment;
    }
}
