namespace BookWorm.Rating.Domain;

public sealed class Feedback(Guid bookId, int rating, string? comment, Guid userId) : IAggregateRoot
{
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
    public Guid BookId { get; private set; } = Guard.Against.Default(bookId);
    public int Rating { get; private set; } =
        Guard.Against.OutOfRange(rating, nameof(rating), 0, 5);
    public string? Comment { get; private set; } = comment;
    public Guid UserId { get; private set; } = Guard.Against.Default(userId);
    public bool IsHidden { get; private set; }
    public DateOnly CreatedAt { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public void Hide()
    {
        IsHidden = true;
    }
}
