using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Categories;
using BookWorm.Catalog.Features.Publishers;

namespace BookWorm.Catalog.Features.Books;

internal sealed record CachedBook(
    BookId Id,
    string? Name,
    string? Description,
    string? Image,
    decimal Price,
    decimal? PriceSale,
    Status Status,
    CategoryDto? Category,
    PublisherDto? Publisher,
    IReadOnlyList<AuthorDto> Authors,
    double AverageRating,
    int TotalReviews,
    long Version
)
{
    public static CachedBook FromBook(Book book)
    {
        return new(
            book.Id,
            book.Name,
            book.Description,
            book.Image,
            book.Price?.OriginalPrice ?? 0,
            book.Price?.DiscountPrice,
            book.Status,
            book.Category?.ToCategoryDto(),
            book.Publisher?.ToPublisherDto(),
            book.BookAuthors.Select(x => x.Author).OfType<Author>().ToAuthorDtos(),
            book.AverageRating,
            book.TotalReviews,
            book.RowVersion
        );
    }
}

internal sealed record CachedBookPage(IReadOnlyList<CachedBook> Items, long TotalItems);
