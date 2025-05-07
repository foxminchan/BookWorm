namespace BookWorm.Catalog.Features.Categories.Update;

public sealed class UpdateCategoryEndpoint : IEndpoint<NoContent, UpdateCategoryCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/categories",
                async (UpdateCategoryCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPut()
            .WithTags(nameof(Category))
            .WithName(nameof(UpdateCategoryEndpoint))
            .WithSummary("Update Category")
            .WithDescription("Update a category if it exists")
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
