using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Categories;
using BookWorm.Catalog.Features.Publishers;

namespace BookWorm.Catalog.Features.Books;

[ExcludeFromCodeCoverage]
public sealed class DomainToDtoMapper(IBlobService blobService) : IMapper<Book, BookDto>
{
    public BookDto MapToDto(Book book)
    {
        var imageUrl = book.Image is not null ? blobService.GetFileUrl(book.Image) : null;

        return new(
            book.Id,
            book.Name,
            book.Description,
            imageUrl,
            book.Price!.OriginalPrice,
            book.Price!.DiscountPrice,
            book.Status,
            book.Category?.ToCategoryDto(),
            book.Publisher?.ToPublisherDto(),
            [.. book.BookAuthors.Select(x => x.Author.ToAuthorDto())],
            book.AverageRating,
            book.TotalReviews
        );
    }

    public IReadOnlyList<BookDto> MapToDtos(IReadOnlyList<Book> models)
    {
        return [.. models.Select(MapToDto)];
    }
}

public static class Extensions
{
    public static void AddBookDomainToDtoMapper(this IServiceCollection services)
    {
        services.AddScoped<IMapper<Book, BookDto>, DomainToDtoMapper>();
    }
}
