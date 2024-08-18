using Ardalis.Result;
using BookWorm.Catalog.Domain.BookAggregate;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Catalog.Features.Books.List;

public sealed record ListBooksRequest(
    int PageIndex,
    int PageSize,
    string? OrderBy,
    bool IsDescending,
    Status[]? Statuses,
    Guid[]? CategoryId,
    Guid[]? PublisherId,
    Guid[]? AuthorIds,
    string? Search);

public sealed record ListBooksResponse(
    PagedInfo PagedInfo,
    List<BookDto> Books);

public sealed class ListBooksEndpoint : IEndpoint<Ok<ListBooksResponse>, ListBooksRequest, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/books",
                async (ISender sender,
                    string? orderBy,
                    Status[]? statuses,
                    Guid[]? categoryId,
                    Guid[]? publisherId,
                    Guid[]? authorIds,
                    string? search,
                    int pageIndex = 1,
                    int pageSize = 20,
                    bool isDescending = false) => await HandleAsync(
                    new(pageIndex, pageSize, orderBy, isDescending, statuses, categoryId, publisherId, authorIds,
                        search),
                    sender))
            .Produces<Ok<ListBooksResponse>>()
            .ProducesValidationProblem()
            .WithTags(nameof(Book))
            .WithName("List Books")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<ListBooksResponse>> HandleAsync(ListBooksRequest request, ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListBooksQuery(
            request.PageIndex,
            request.PageSize,
            request.OrderBy,
            request.IsDescending,
            request.Statuses,
            request.CategoryId,
            request.PublisherId,
            request.AuthorIds,
            request.Search), cancellationToken);

        var response = new ListBooksResponse(
            result.PagedInfo,
            result.Value.ToBookDtos());

        return TypedResults.Ok(response);
    }
}
