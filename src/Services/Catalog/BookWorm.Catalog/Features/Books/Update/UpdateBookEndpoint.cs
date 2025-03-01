namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookEndpoint : IEndpoint<NoContent, UpdateBookCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/books/{id:guid}",
                async (Guid id, [AsParameters] UpdateBookCommand command, ISender sender) =>
                    await HandleAsync(command with { Id = id }, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .DisableAntiforgery()
            .WithOpenApi()
            .WithTags(nameof(Book))
            .WithFormOptions(true)
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<NoContent> HandleAsync(
        UpdateBookCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
