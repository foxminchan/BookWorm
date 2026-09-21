using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Categories.Delete;

internal sealed class DeleteCategoryEndpoint : IEndpoint<NoContent, CategoryId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/categories/{id}",
                async (
                    [Description("The unique identifier of the category to be deleted")]
                        CategoryId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Category))
            .WithName(nameof(DeleteCategoryEndpoint))
            .WithSummary("Delete Category")
            .WithDescription("Delete a category from the catalog system")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        CategoryId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteCategoryCommand((Guid)id), cancellationToken);

        return TypedResults.NoContent();
    }
}
