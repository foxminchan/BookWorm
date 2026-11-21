namespace BookWorm.Catalog.Features.Books;

[ExcludeFromCodeCoverage]
internal sealed class DomainToDtoMapper(IBlobService blobService)
    : IMapper<Book, BookDto>,
        IMapper<CachedBook, BookDto>
{
    public BookDto Map(Book book)
    {
        return Map(CachedBook.FromBook(book));
    }

    public IReadOnlyList<BookDto> Map(IReadOnlyList<Book> models)
    {
        return models.Count == 0 ? [] : [.. models.Select(Map)];
    }

    public BookDto Map(CachedBook book)
    {
        var imageUrl = book.Image is not null ? blobService.GetFileSasUrl(book.Image) : null;

        return new(
            book.Id,
            book.Name,
            book.Description,
            imageUrl,
            book.Price,
            book.PriceSale,
            book.Status,
            book.Category,
            book.Publisher,
            book.Authors,
            book.AverageRating,
            book.TotalReviews,
            book.Version
        );
    }

    public IReadOnlyList<BookDto> Map(IReadOnlyList<CachedBook> models)
    {
        return models.Count == 0 ? [] : [.. models.Select(Map)];
    }
}
