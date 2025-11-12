namespace BookWorm.McpTools.Models;

public record Feedback(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    Guid BookId
);
