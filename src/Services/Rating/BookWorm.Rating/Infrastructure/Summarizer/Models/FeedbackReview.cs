namespace BookWorm.Rating.Infrastructure.Summarizer.Models;

internal sealed record FeedbackReview(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    Guid BookId
);
