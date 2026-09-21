using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Books.Get;

internal sealed class GetBookEndpoint : IEndpoint<Ok<BookDto>, BookId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/books/{id}",
                async (
                    [Description("The unique identifier of the book to be retrieved")] BookId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesGet<BookDto>(hasNotFound: true)
            .WithTags(nameof(Book))
            .WithName(nameof(GetBookEndpoint))
            .WithSummary("Get Book")
            .WithDescription("Get a book by identifier")
            .MapToApiVersion(ApiVersions.V1);
    }

    public async Task<Ok<BookDto>> HandleAsync(
        BookId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new GetBookQuery(id), cancellationToken);

        return TypedResults.Ok(result);
    }
}
