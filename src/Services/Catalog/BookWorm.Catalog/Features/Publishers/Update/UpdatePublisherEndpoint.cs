namespace BookWorm.Catalog.Features.Publishers.Update;

public sealed class UpdatePublisherEndpoint : IEndpoint<NoContent, UpdatePublisherCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/publishers/{id:guid}",
                async (Guid id, UpdatePublisherCommand command, ISender sender) =>
                    await HandleAsync(command with { Id = id }, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Publisher))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<NoContent> HandleAsync(
        UpdatePublisherCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
