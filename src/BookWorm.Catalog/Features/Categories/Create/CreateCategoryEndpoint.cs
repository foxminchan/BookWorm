using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Categories.Create;

public sealed class CreateCategoryEndpoint
    : IEndpoint<Created<Guid>, CreateCategoryCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/categories",
                async (CreateCategoryCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Category))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder()
                .WithVersion()
                .WithResource(nameof(Categories))
                .WithId(result.Value)
                .Build(),
            result.Value
        );
    }
}
