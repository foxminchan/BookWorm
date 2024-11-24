using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Features.Books.Delete;

public sealed class DeleteBookEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/books/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Book))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteBookCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
