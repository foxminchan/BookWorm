using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Authors.List;
using BookWorm.Catalog.Features.Books;
using BookWorm.Catalog.Features.Books.Get;
using BookWorm.Catalog.Features.Books.List;
using BookWorm.Catalog.Features.Categories.List;
using BookWorm.Catalog.Features.Dashboard;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Status = BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Status;

namespace BookWorm.Catalog.Grpc.Services;

internal sealed class BookService(
    IBookRepository repository,
    ILogger<BookService> logger,
    ISender sender
) : BookGrpcService.BookGrpcServiceBase
{
    [Authorize(Policy = Authorization.Policies.Admin)]
    public override async Task<GetDashboardResponse> GetDashboard(
        GetDashboardRequest request,
        ServerCallContext context
    )
    {
        var summary = await sender.Send(new GetCatalogDashboardQuery(), context.CancellationToken);
        return new()
        {
            TotalBooks = summary.TotalBooks,
            Categories =
            {
                summary.Categories.Select(category => new DashboardCategory
                {
                    Name = category.Name,
                    Value = category.Value,
                }),
            },
        };
    }

    [AllowAnonymous]
    public override async Task<GetBookResponse> GetBook(
        GetBookRequest request,
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

        var book = await repository.GetSummaryByIdAsync(
            Guid.Parse(request.BookId),
            context.CancellationToken
        )
            ?? throw new RpcException(
                new(StatusCode.NotFound, $"Book with id {request.BookId} not found.")
            );

        return MapToBookResponse(book);
    }

    [AllowAnonymous]
    public override async Task<GetBooksResponse> GetBooks(
        GetBooksRequest request,
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

        var books = await repository.ListSummariesAsync(
            new BookFilterSpec([.. request.BookIds.Select(Guid.Parse)]),
            context.CancellationToken
        );

        return MapToBookResponse(books);
    }

    [AllowAnonymous]
    public override async Task<ListBooksResponse> ListBooks(
        ListBooksRequest request,
        ServerCallContext context
    )
    {
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search;
        var result = await sender.Send(
            new ListBooksQuery(Search: search),
            context.CancellationToken
        );

        return new() { Books = { result.Select(MapToBookItem) } };
    }

    [AllowAnonymous]
    public override async Task<GetBookDetailsResponse> GetBookDetails(
        GetBookDetailsRequest request,
        ServerCallContext context
    )
    {
        if (!Guid.TryParse(request.BookId, out var bookId))
        {
            throw new RpcException(
                new(StatusCode.InvalidArgument, $"Invalid book id '{request.BookId}'.")
            );
        }

        try
        {
            var book = await sender.Send(
                new GetBookQuery(BookId.From(bookId)),
                context.CancellationToken
            );

            return new() { Book = MapToBookItem(book) };
        }
        catch (NotFoundException)
        {
            throw new RpcException(
                new(StatusCode.NotFound, $"Book with id {request.BookId} not found.")
            );
        }
    }

    [AllowAnonymous]
    public override async Task<ListCategoriesResponse> ListCategories(
        ListCategoriesRequest request,
        ServerCallContext context
    )
    {
        var categories = await sender.Send(new ListCategoriesQuery(), context.CancellationToken);

        return new()
        {
            Categories =
            {
                categories.Select(c => new CategoryItem
                {
                    Id = c.Id.ToString(),
                    Name = c.Name ?? string.Empty,
                }),
            },
        };
    }

    [AllowAnonymous]
    public override async Task<ListAuthorsResponse> ListAuthors(
        ListAuthorsRequest request,
        ServerCallContext context
    )
    {
        var authors = await sender.Send(new ListAuthorsQuery(), context.CancellationToken);

        return new()
        {
            Authors =
            {
                authors.Select(a => new AuthorItem
                {
                    Id = a.Id.ToString(),
                    Name = a.Name ?? string.Empty,
                }),
            },
        };
    }

    private static BookItem MapToBookItem(BookDto book)
    {
        return new()
        {
            Id = book.Id.ToString(),
            Name = book.Name ?? string.Empty,
            Description = book.Description ?? string.Empty,
            ImageUrl = book.ImageUrl ?? string.Empty,
            Price = book.Price,
            PriceSale = book.PriceSale,
            Status = book.Status == Status.InStock ? BookStatus.InStock : BookStatus.OutOfStock,
            AverageRating = book.AverageRating,
            TotalReviews = book.TotalReviews,
            Category = book.Category is null
                ? null
                : new()
                {
                    Id = book.Category.Id.ToString(),
                    Name = book.Category.Name ?? string.Empty,
                },
            Publisher = book.Publisher is null
                ? null
                : new()
                {
                    Id = book.Publisher.Id.ToString(),
                    Name = book.Publisher.Name ?? string.Empty,
                },
            Authors =
            {
                book.Authors.Select(a => new AuthorItem
                {
                    Id = a.Id.ToString(),
                    Name = a.Name ?? string.Empty,
                }),
            },
        };
    }

    private static GetBookResponse MapToBookResponse(BookSummary book)
    {
        return new()
        {
            Id = book.Id.ToString(),
            Name = book.Name,
            Price = book.OriginalPrice,
            PriceSale = book.DiscountPrice,
            Status = book.Status == Status.InStock ? BookStatus.InStock : BookStatus.OutOfStock,
        };
    }

    private static GetBooksResponse MapToBookResponse(IReadOnlyList<BookSummary> books)
    {
        return new() { Books = { books.Select(MapToBookResponse) } };
    }
}
