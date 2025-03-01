namespace BookWorm.Rating.Features;

public sealed record FeedbackDto(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    Guid BookId
);
