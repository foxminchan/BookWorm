using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books.List;

public sealed class ListBooksEndpoint : IEndpoint<Ok<PagedItems<BookDto>>, ListBooksQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/books",
                async ([AsParameters] ListBooksQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<Ok<PagedItems<BookDto>>>()
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Book))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<PagedItems<BookDto>>> HandleAsync(
        ListBooksQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);

        var response = new PagedItems<BookDto>(result.PagedInfo, result.Value.ToBookDtos());

        return TypedResults.Ok(response);
    }
}
