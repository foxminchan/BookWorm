namespace BookWorm.McpTools.Models;

public sealed record Feedback(
    FeedbackId Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    BookId BookId
);
