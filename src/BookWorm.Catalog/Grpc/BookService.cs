using BookWorm.Catalog.Features.Books.Get;
using Grpc.Core;
using BookModel = BookWorm.Catalog.Domain.BookAggregate.Book;
using GrpcBookServer = BookWorm.Catalog.Grpc.Book.BookBase;

namespace BookWorm.Catalog.Grpc;

public sealed class BookService(ISender sender, ILogger<BookService> logger) : GrpcBookServer
{
    [AllowAnonymous]
    public override async Task<BookResponse> GetBook(BookRequest request, ServerCallContext context)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Getting book with id: {Id}",
                nameof(BookService),
                request.BookId
            );
        }

        var book = await sender.Send(
            new GetBookQuery(Guid.Parse(request.BookId)),
            context.CancellationToken
        );

        return book.Value is not null ? MapToBookResponse(book.Value) : new();
    }

    [AllowAnonymous]
    public override async Task<BookStatusResponse> GetBookStatus(
        BookStatusRequest request,
        ServerCallContext context
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Getting book status with id: {Id}",
                nameof(BookService),
                request.BookId
            );
        }

        var book = await sender.Send(
            new GetBookQuery(Guid.Parse(request.BookId)),
            context.CancellationToken
        );

        return book.Value is not null ? MapToBookStatusResponse(book.Value) : new();
    }

    private static BookResponse MapToBookResponse(BookModel book)
    {
        return new()
        {
            Book = new()
            {
                Id = book.Id.ToString(),
                Name = book.Name,
                Price = decimal.ToDouble(book.Price!.OriginalPrice),
                PriceSale = decimal.ToDouble(book.Price.DiscountPrice ?? -1m),
            },
        };
    }

    private static BookStatusResponse MapToBookStatusResponse(BookModel book)
    {
        return new()
        {
            BookStatus = new() { Id = book.Id.ToString(), Status = book.Status.ToString() },
        };
    }
}
