using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books;

public sealed record BookDto(
    Guid Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    decimal PriceSale,
    Status Status,
    string? Category,
    string? Publisher,
    List<string?> Authors,
    double AverageRating,
    int RatingsCount
);
