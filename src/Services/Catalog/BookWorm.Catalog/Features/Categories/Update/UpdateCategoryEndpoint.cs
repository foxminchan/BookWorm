namespace BookWorm.Catalog.Features.Categories.Update;

public sealed class UpdateCategoryEndpoint : IEndpoint<NoContent, UpdateCategoryCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/categories/{id:guid}",
                async (Guid id, UpdateCategoryCommand command, ISender sender) =>
                    await HandleAsync(command with { Id = id }, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Category))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<NoContent> HandleAsync(
        UpdateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
