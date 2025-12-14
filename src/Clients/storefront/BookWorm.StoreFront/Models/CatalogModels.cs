namespace BookWorm.StoreFront.Models;

public sealed record Book(
    Guid Id,
    string Title,
    string? Description,
    decimal Price,
    string? CoverImageUrl,
    string? Publisher,
    List<string> Authors,
    List<string> Categories,
    DateTime PublishedDate
);

public sealed record Publisher(Guid Id, string Name);

public sealed record Author(Guid Id, string Name);

public sealed record Category(Guid Id, string Name);

public sealed record CatalogListResponse(List<Book> Items, int TotalCount);
