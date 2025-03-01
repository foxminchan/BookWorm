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
            .WithOpenApi()
            .WithTags(nameof(Book))
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
