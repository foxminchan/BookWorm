using Mediator;

namespace BookWorm.Ordering.Features.Orders.Delete;

internal sealed class DeleteOrderEndpoint : IEndpoint<NoContent, OrderId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/orders/{id}",
                async (
                    [Description("The unique identifier of the order to be deleted")] OrderId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Order))
            .WithName(nameof(DeleteOrderEndpoint))
            .WithSummary("Delete Order")
            .WithDescription("Delete an order if it exists")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        OrderId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteOrderCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
