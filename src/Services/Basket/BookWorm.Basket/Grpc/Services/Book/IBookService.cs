using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Basket.Grpc.Services.Book;

public interface IBookService
{
    Task<BookResponse?> GetBookByIdAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        CancellationToken cancellationToken = default
    );

    Task<BooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    );
}
