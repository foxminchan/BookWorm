namespace BookWorm.Catalog.Features.Books.Delete;

public sealed class DeleteBookEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/books/{id:guid}",
                async (
                    [Description("The unique identifier of the book to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Book))
            .WithName(nameof(DeleteBookEndpoint))
            .WithSummary("Delete Book")
            .WithDescription("Delete a book if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
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
