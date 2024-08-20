using BookWorm.Catalog.Domain.BookAggregate.Specifications;
using Grpc.Core;
using GrpcBookServer = BookWorm.Catalog.Grpc.Book.BookBase;
using BookModel = BookWorm.Catalog.Domain.BookAggregate.Book;

namespace BookWorm.Catalog.Grpc;

public sealed class BookService(IReadRepository<BookModel> repository, ILogger<BookService> logger) : GrpcBookServer
{
    [AllowAnonymous]
    public override async Task<BookResponse> GetBook(BookRequest request, ServerCallContext context)
    {
        BookFilterSpec spec = new(Guid.Parse(request.BookId));

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[{Service}] - Getting book with id: {Id}", nameof(BookService), request.BookId);
        }

        var book = await repository.FirstOrDefaultAsync(spec);

        if (book is null)
        {
            ThrowNotFound();
        }

        return MapToBookResponse(book);
    }

    [AllowAnonymous]
    public override async Task<BookStatusResponse> GetBookStatus(BookStatusRequest request, ServerCallContext context)
    {
        BookFilterSpec spec = new(Guid.Parse(request.BookId));

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[{Service}] - Getting book status with id: {Id}", nameof(BookService), request.BookId);
        }

        var book = await repository.FirstOrDefaultAsync(spec);

        if (book is null)
        {
            ThrowNotFound();
        }

        return MapToBookStatusResponse(book);
    }

    [DoesNotReturn]
    private static void ThrowNotFound()
    {
        throw new RpcException(new(StatusCode.NotFound, "Book not found"));
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
                PriceSale = decimal.ToDouble(book.Price.DiscountPrice ?? -1m)
            }
        };
    }

    private static BookStatusResponse MapToBookStatusResponse(BookModel book)
    {
        return new() { BookStatus = new() { Id = book.Id.ToString(), Status = book.Status.ToString() } };
    }
}
