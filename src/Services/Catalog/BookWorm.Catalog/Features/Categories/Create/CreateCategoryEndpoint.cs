namespace BookWorm.Catalog.Features.Categories.Create;

public sealed class CreateCategoryEndpoint : IEndpoint<Ok<Guid>, CreateCategoryCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/categories",
                async (CreateCategoryCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Guid>()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .WithTags(nameof(Category))
            .WithName(nameof(CreateCategoryEndpoint))
            .WithSummary("Create Category")
            .WithDescription("Create a new category in the catalog system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<Ok<Guid>> HandleAsync(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}
