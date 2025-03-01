namespace BookWorm.Catalog.Features.Authors.Update;

public sealed class UpdateAuthorEndpoint : IEndpoint<NoContent, UpdateAuthorCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/authors/{id:guid}",
                async (Guid id, UpdateAuthorCommand command, ISender sender) =>
                    await HandleAsync(command with { Id = id }, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Author))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<NoContent> HandleAsync(
        UpdateAuthorCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
