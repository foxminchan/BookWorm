using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Categories.Delete;

public sealed class DeleteCategoryEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/categories/{id:guid}",
                async (
                    [Description("The unique identifier of the category to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Category))
            .WithName(nameof(DeleteCategoryEndpoint))
            .WithSummary("Delete Category")
            .WithDescription("Delete a category from the catalog system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteCategoryCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
