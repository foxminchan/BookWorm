using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Categories;
using BookWorm.Catalog.Features.Publishers;

namespace BookWorm.Catalog.Features.Books;

public sealed record BookDto(
    Guid Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    decimal? PriceSale,
    Status Status,
    CategoryDto? Category,
    PublisherDto? Publisher,
    IReadOnlyList<AuthorDto> Authors,
    double AverageRating,
    int TotalReviews
);
