using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Ordering.Grpc.Services.Book;

public interface IBookService
{
    Task<GetBookResponse?> GetBookByIdAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id,
        CancellationToken cancellationToken = default
    );

    Task<GetBooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    );
}
