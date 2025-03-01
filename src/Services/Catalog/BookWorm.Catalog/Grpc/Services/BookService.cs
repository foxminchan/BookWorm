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

        return book is not null ? MapToBookResponse(book) : new();
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
            Price = decimal.ToDouble(book.Price!.OriginalPrice),
            PriceSale = double.TryParse(book.Price?.DiscountPrice?.ToString(), out var price)
                ? price
                : null,
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
