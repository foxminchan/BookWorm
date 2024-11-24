using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed class DeleteAuthorEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/authors/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Author))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteAuthorCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
