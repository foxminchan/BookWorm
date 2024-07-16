using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books;

public static class EntityToDto
{
    public static BookDto ToBookDto(this Book book)
    {
        return new(book.Id,
            book.Name,
            book.Description,
            book.ImageUrl,
            book.Price!.OriginalPrice,
            book.Price!.DiscountPrice ?? -1,
            book.Status,
            book.Category?.Name,
            book.Publisher?.Name,
            book.BookAuthors.Select(x => x.Author.Name).ToList(),
            book.AverageRating,
            book.TotalReviews);
    }

    public static List<BookDto> ToBookDtos(this IEnumerable<Book> books)
    {
        return books.Select(ToBookDto).ToList();
    }
}
