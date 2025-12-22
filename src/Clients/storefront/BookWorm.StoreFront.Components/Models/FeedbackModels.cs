namespace BookWorm.StoreFront.Components.Models;

public sealed record Feedback(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    Guid BookId
);

public sealed record CreateFeedbackRequest(
    Guid BookId,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating
);
