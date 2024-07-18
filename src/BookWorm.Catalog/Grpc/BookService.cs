using System.Diagnostics.CodeAnalysis;
using BookWorm.Core.SharedKernel;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Catalog.Grpc;

public sealed class BookService(IReadRepository<Domain.BookAggregate.Book> repository) : Book.BookBase
{
    [AllowAnonymous]
    public override async Task<BookResponse> GetBook(BookRequest request, ServerCallContext context)
    {
        var book = await repository.GetByIdAsync(Guid.Parse(request.BookId));

        if (book is null)
        {
            ThrowNotFound();
        }

        return MapToBookResponse(book);
    }

    [AllowAnonymous]
    public override async Task<BookStatusResponse> GetBookStatus(BookStatusRequest request, ServerCallContext context)
    {
        var book = await repository.GetByIdAsync(Guid.Parse(request.BookId));

        if (book is null)
        {
            ThrowNotFound();
        }

        return MapToBookStatusResponse(book);
    }

    [DoesNotReturn]
    private static void ThrowNotFound() => throw new RpcException(new(StatusCode.NotFound, "Book not found"));

    private static BookResponse MapToBookResponse(Domain.BookAggregate.Book book)
    {
        return new()
        {
            Book = new()
            {
                Id = book.Id.ToString(),
                Name = book.Name,
                Price = decimal.ToDouble(book.Price!.OriginalPrice),
                PriceSale = decimal.ToDouble(book.Price.DiscountPrice ?? -1m),
            }
        };
    }

    private static BookStatusResponse MapToBookStatusResponse(Domain.BookAggregate.Book book)
    {
        return new() { BookStatus = new() { Id = book.Id.ToString(), Status = book.Status.ToString() } };
    }
}
