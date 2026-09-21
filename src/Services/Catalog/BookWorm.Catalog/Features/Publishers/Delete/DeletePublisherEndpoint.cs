using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Publishers.Delete;

internal sealed class DeletePublisherEndpoint : IEndpoint<NoContent, PublisherId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/publishers/{id}",
                async (
                    [Description("The unique identifier of the publisher to be deleted")]
                        PublisherId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Publisher))
            .WithName(nameof(DeletePublisherEndpoint))
            .WithSummary("Delete Publisher")
            .WithDescription("Delete a publisher from the catalog system")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        PublisherId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeletePublisherCommand((Guid)id), cancellationToken);

        return TypedResults.NoContent();
    }
}
