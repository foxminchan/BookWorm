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
        var response = new BookResponse
        {
            Id = book.Id.ToString(),
            Name = book.Name,
            Price = ToDecimal(book.Price!.OriginalPrice),
            Status = book.Status == Status.InStock ? BookStatus.InStock : BookStatus.OutOfStock,
        };

        // Only set PriceSale if there's a discount price
        if (book.Price?.DiscountPrice is not null)
        {
            response.PriceSale = ToDecimal(book.Price.DiscountPrice.Value);
        }

        return response;
    }

    private static BooksResponse MapToBookResponse(IReadOnlyList<Book> books)
    {
        var response = new BooksResponse();

        response.Books.AddRange(books.Select(MapToBookResponse));

        return response;
    }

    /// <summary>
    /// Converts a .NET decimal to a protobuf Decimal message.
    /// Uses Google's protobuf decimal representation where nanos is always positive.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    /// <returns>A protobuf Decimal message.</returns>
    public static Decimal ToDecimal(decimal value)
    {
        // For negative numbers, we need to use floor division
        // to ensure nanos is always positive
        var units = (long)Math.Floor(value);
        var fractionalPart = value - units;
        
        // Convert fractional part to nanoseconds (9 decimal places)
        // fractionalPart is always >= 0 after floor division
        var nanos = (int)Math.Round(fractionalPart * 1_000_000_000m);
        
        // Handle cases where rounding causes nanos to be 1 billion
        if (nanos >= 1_000_000_000)
        {
            units += 1;
            nanos = 0;
        }
        
        return new Decimal
        {
            Units = units,
            Nanos = nanos
        };
    }

    /// <summary>
    /// Converts a protobuf Decimal message to a .NET decimal.
    /// </summary>
    /// <param name="value">The protobuf Decimal to convert.</param>
    /// <returns>A .NET decimal value.</returns>
    public static decimal FromDecimal(Decimal value)
    {
        if (value is null)
        {
            return 0m;
        }
        
        // Convert nanos back to fractional part (always positive)
        var fractionalPart = (decimal)value.Nanos / 1_000_000_000m;
        
        // Combine units and fractional part
        return value.Units + fractionalPart;
    }
}
