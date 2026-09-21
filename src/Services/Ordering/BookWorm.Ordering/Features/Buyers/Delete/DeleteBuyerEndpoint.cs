using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Delete;

internal sealed class DeleteBuyerEndpoint : IEndpoint<NoContent, BuyerId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/buyers/{id}",
                async (
                    [Description("The unique identifier of the buyer to be deleted")] BuyerId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Buyer))
            .WithName(nameof(DeleteBuyerEndpoint))
            .WithSummary("Delete Buyer")
            .WithDescription("Delete a buyer by ID if it exists")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        BuyerId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteBuyerCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
