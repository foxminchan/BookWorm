using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Categories.Create;

internal sealed class CreateCategoryEndpoint
    : IEndpoint<Ok<CategoryId>, CreateCategoryCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/categories",
                async (CreateCategoryCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPostWithoutLocation<CategoryId>()
            .WithTags(nameof(Category))
            .WithName(nameof(CreateCategoryEndpoint))
            .WithSummary("Create Category")
            .WithDescription("Create a new category in the catalog system")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<CategoryId>> HandleAsync(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(CategoryId.From(result));
    }
}
