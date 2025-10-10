using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Catalog.Features.Publishers.Update;

public sealed class UpdatePublisherEndpoint : IEndpoint<NoContent, UpdatePublisherCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/publishers",
                async (UpdatePublisherCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPut()
            .WithTags(nameof(Publisher))
            .WithName(nameof(UpdatePublisherEndpoint))
            .WithSummary("Update Publisher")
            .WithDescription("Updates publisher if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        UpdatePublisherCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
