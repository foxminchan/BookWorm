using BookWorm.Constants.Core;

namespace BookWorm.Ordering.Features.Buyers.Delete;

public sealed class DeleteBuyerEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/buyers/{id:guid}",
                async (
                    [Description("The unique identifier of the buyer to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Buyer))
            .WithName(nameof(DeleteBuyerEndpoint))
            .WithSummary("Delete Buyer")
            .WithDescription("Delete a buyer by ID if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteBuyerCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
