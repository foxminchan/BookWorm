namespace BookWorm.StoreFront.Components.Models;

public sealed record Book(
    Guid Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    decimal? PriceSale,
    string Status,
    Category? Category,
    Publisher? Publisher,
    IReadOnlyList<Author> Authors,
    double AverageRating,
    int TotalReviews,
    DateTime? CreatedAt
);

public sealed record Publisher(Guid Id, string Name);

public sealed record Author(Guid Id, string Name);

public sealed record Category(Guid Id, string Name);

public sealed record CatalogListResponse(List<Book> Items, int TotalCount);
