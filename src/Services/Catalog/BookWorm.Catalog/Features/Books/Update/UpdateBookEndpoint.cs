namespace BookWorm.Catalog.Features.Books.Update;

public sealed class UpdateBookEndpoint : IEndpoint<NoContent, UpdateBookCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/books",
                async ([AsParameters] UpdateBookCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Accepts<UpdateBookCommand>(MediaTypeNames.Multipart.FormData)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithTags(nameof(Book))
            .WithName(nameof(UpdateBookEndpoint))
            .WithSummary("Update Book")
            .WithDescription("Update a book if it exists")
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
