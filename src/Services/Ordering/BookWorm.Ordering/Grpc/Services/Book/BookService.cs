using BookWorm.Catalog.Grpc.Services;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc.Services.Book;

public sealed class BookService(BookGrpcServiceClient service, ILogger<BookService> logger)
    : IBookService
{
    public async Task<BookResponse?> GetBookByIdAsync(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Begin grpc call {Method} with {BookId}",
                nameof(BookService),
                nameof(GetBookByIdAsync),
                id
            );
        }

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
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Begin grpc call {Method} with {BookIds}",
                nameof(BookService),
                nameof(GetBooksByIdsAsync),
                ids
            );
        }

        var result = await service.GetBooksAsync(
            new() { BookIds = { ids } },
            cancellationToken: cancellationToken
        );

        return result;
    }
}
