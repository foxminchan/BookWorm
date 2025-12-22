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
    int TotalReviews
);

public sealed record Publisher(Guid Id, string Name);

public sealed record Author(Guid Id, string Name);

public sealed record Category(Guid Id, string Name);

public sealed class CatalogQueryParams
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string OrderBy { get; set; } = nameof(Book.Price);
    public bool IsDescending { get; set; }
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid[]? CategoryId { get; set; }
    public Guid[]? PublisherId { get; set; }
    public Guid[]? AuthorIds { get; set; }
}
