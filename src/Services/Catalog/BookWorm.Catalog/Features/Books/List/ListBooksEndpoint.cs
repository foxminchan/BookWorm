using BookWorm.SharedKernel.Results;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Catalog.Features.Books.List;

internal sealed class ListBooksEndpoint
    : IEndpoint<Ok<PagedResult<BookDto>>, ListBooksQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapQuery("/books", async ([FromBody] ListBooksQuery? query, ISender sender) =>
                await HandleAsync(query ?? new(), sender)
            )
            .WithPaginationHeaders()
            .ProducesQuery<PagedResult<BookDto>>(true)
            .WithTags(nameof(Book))
            .WithName(nameof(ListBooksEndpoint))
            .WithSummary("List Books")
            .WithDescription("List all books with advanced filtering and pagination")
            .MapToApiVersion(ApiVersions.V1);
    }

    public async Task<Ok<PagedResult<BookDto>>> HandleAsync(
        ListBooksQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}
