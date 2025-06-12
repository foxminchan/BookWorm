using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Status = BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Status;

namespace BookWorm.Catalog.Grpc.Services;

public sealed class BookService(IBookRepository repository, ILogger<BookService> logger)
    : BookGrpcService.BookGrpcServiceBase
{
    [AllowAnonymous]
    public override async Task<BookResponse> GetBook(BookRequest request, ServerCallContext context)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Getting book status with id: {Id}",
                nameof(BookService),
                request.BookId
            );
        }

        var book = await repository.GetByIdAsync(Guid.Parse(request.BookId));

        if (book is null)
        {
            throw new RpcException(
                new(StatusCode.NotFound, $"Book with id {request.BookId} not found.")
            );
        }

        return MapToBookResponse(book);
    }

    [AllowAnonymous]
    public override async Task<BooksResponse> GetBooks(
        BooksRequest request,
        ServerCallContext context
    )
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{Service}] - Getting book status with id: {Id}",
                nameof(BookService),
                request.BookIds
            );
        }

        var books = await repository.ListAsync(
            new BookFilterSpec(request.BookIds.Select(Guid.Parse).ToArray())
        );

        return MapToBookResponse(books);
    }

    private static BookResponse MapToBookResponse(Book book)
    {
        return new()
        {
            Id = book.Id.ToString(),
            Name = book.Name,
            Price = book.Price!.OriginalPrice,
            PriceSale = book.Price?.DiscountPrice,
            Status = book.Status == Status.InStock ? BookStatus.InStock : BookStatus.OutOfStock,
        };
    }

    private static BooksResponse MapToBookResponse(IReadOnlyList<Book> books)
    {
        var response = new BooksResponse();

        response.Books.AddRange(books.Select(MapToBookResponse));

        return response;
    }
}
