namespace BookWorm.McpTools.Models;

public sealed record Feedback(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    Guid BookId
);
