using BookWorm.SharedKernel.Results;

namespace BookWorm.Catalog.Features.Books.List;

public sealed class ListBooksEndpoint : IEndpoint<Ok<PagedResult<BookDto>>, ListBooksQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/books",
                async ([AsParameters] ListBooksQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<PagedResult<BookDto>>()
            .ProducesValidationProblem()
            .WithTags(nameof(Book))
            .WithName(nameof(ListBooksEndpoint))
            .WithSummary("List Books")
            .WithDescription("List all books with advanced filtering and pagination")
            .MapToApiVersion(new(1, 0));
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
