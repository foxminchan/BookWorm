using BookWorm.Catalog.Grpc.Services;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc.Services.Book;

[ExcludeFromCodeCoverage]
public sealed class BookService(BookGrpcServiceClient service) : IBookService
{
    public async Task<BookResponse?> GetBookByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await service.GetBookAsync(
            new() { BookId = id },
            cancellationToken: cancellationToken
        );

        return result;
    }

    public async Task<BooksResponse?> GetBooksByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    )
    {
        var result = await service.GetBooksAsync(
            new() { BookIds = { ids } },
            cancellationToken: cancellationToken
        );

        return result;
    }
}
