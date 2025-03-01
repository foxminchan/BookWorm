namespace BookWorm.Catalog.Features.Authors.List;

public sealed class ListAuthorsEndpoint : IEndpoint<Ok<IReadOnlyList<AuthorDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/authors", async (ISender sender) => await HandleAsync(sender))
            .Produces<IReadOnlyList<AuthorDto>>()
            .WithOpenApi()
            .WithTags(nameof(Author))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<IReadOnlyList<AuthorDto>>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new ListAuthorsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
