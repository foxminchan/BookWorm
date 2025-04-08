namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed class DeleteAuthorEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/authors/{id:guid}",
                async (
                    [Description("The unique identifier of the author to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithTags(nameof(Author))
            .WithName(nameof(DeleteAuthorEndpoint))
            .WithSummary("Delete Author")
            .WithDescription("Delete an author from the catalog system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
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
