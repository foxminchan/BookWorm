namespace BookWorm.Ordering.Features.Orders.Delete;

public sealed class DeleteOrderEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/orders/{id:guid}",
                async (
                    [Description("The unique identifier of the order to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Order))
            .WithName(nameof(DeleteOrderEndpoint))
            .WithSummary("Delete Order")
            .WithDescription("Delete an order if it exists")
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
        await sender.Send(new DeleteOrderCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}
