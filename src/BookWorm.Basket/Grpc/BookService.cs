using GrpcBookClient = BookWorm.Catalog.Grpc.Book.BookClient;
using GrpcBookItem = BookWorm.Catalog.Grpc.BookItem;
using GrpcBookStatus = BookWorm.Catalog.Grpc.BookStatus;

namespace BookWorm.Basket.Grpc;

public sealed class BookService(GrpcBookClient bookClient, ILogger<BookService> logger)
    : IBookService
{
    public async Task<BookStatus> GetBookStatusAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Begin grpc call {Method} with {BookId}",
                nameof(BookService),
                nameof(GetBookStatusAsync),
                bookId
            );
        }

        var response = await bookClient.GetBookStatusAsync(
            new() { BookId = bookId.ToString() },
            cancellationToken: cancellationToken
        );

        return MapBookStatus(response.BookStatus);
    }

    public async Task<bool> IsBookExistsAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Begin grpc call {Method} with {BookId}",
                nameof(BookService),
                nameof(IsBookExistsAsync),
                bookId
            );
        }

        var response = await bookClient.GetBookAsync(
            new() { BookId = bookId.ToString() },
            cancellationToken: cancellationToken
        );

        return response.Book is not null;
    }

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

    private static BookStatus MapBookStatus(GrpcBookStatus status)
    {
        return new(Guid.Parse(status.Id), status.Status);
    }

    private static BookItem MapBookItem(GrpcBookItem book)
    {
        return new(Guid.Parse(book.Id), book.Name, (decimal)book.Price, (decimal)book.PriceSale);
    }
}

public sealed record BookStatus(Guid Id, string Status);

public sealed record BookItem(Guid Id, string Name, decimal Price, decimal PriceSale);
