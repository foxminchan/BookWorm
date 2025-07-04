using BookWorm.Constants.Core;

namespace BookWorm.Catalog.Features.Publishers.Delete;

public sealed class DeletePublisherEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/publishers/{id:guid}",
                async (
                    [Description("The unique identifier of the publisher to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Publisher))
            .WithName(nameof(DeletePublisherEndpoint))
            .WithSummary("Delete Publisher")
            .WithDescription("Delete a publisher from the catalog system")
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
        await sender.Send(new DeletePublisherCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
