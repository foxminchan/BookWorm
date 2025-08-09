namespace BookWorm.McpTools.Models;

public sealed record Feedback(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Comment,
    int Rating,
    Guid BookId
);

public sealed class FeedbackQueryParams
{
    public required Guid BookId { get; set; }
    public int PageSize { get; set; } = int.MaxValue;
}
