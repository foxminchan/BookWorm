using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;
using GrpcBookItem = BookWorm.Catalog.Grpc.BookItem;

namespace BookWorm.Ordering.Grpc;

public sealed class BookService(GrpcBookClient bookClient, ILogger<BookService> logger)
    : IBookService
{
    public async Task<BookItem> GetBookAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Begin grpc call {Method} with {BookId}",
                nameof(BookService),
                nameof(GetBookAsync),
                bookId
            );
        }

        var response = await bookClient.GetBookAsync(
            new() { BookId = bookId.ToString() },
            cancellationToken: cancellationToken
        );

        return MapBookItem(response.Book);
    }

    private static BookItem MapBookItem(GrpcBookItem book)
    {
        return new(Guid.Parse(book.Id), book.Name);
    }
}

public sealed record BookItem(Guid Id, string Name);
