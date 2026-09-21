namespace BookWorm.McpTools.Models;

public sealed record Book(
    BookId Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    decimal? PriceSale,
    Category? Category,
    Publisher? Publisher,
    IReadOnlyList<Author> Authors,
    double AverageRating,
    int TotalReviews
);
