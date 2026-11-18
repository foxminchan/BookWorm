namespace BookWorm.Ordering.Features.Orders.GetHistory;

internal sealed class GetOrderHistoryEndpoint
    : IEndpoint<Ok<IReadOnlyList<OrderEventHistoryDto>>, OrderId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id}/history",
                async (
                    [Description("The unique identifier of the order")] OrderId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesGet<IReadOnlyList<OrderEventHistoryDto>>(hasNotFound: true)
            .WithTags(nameof(Order))
            .WithName(nameof(GetOrderHistoryEndpoint))
            .WithSummary("Get Order Event History")
            .WithDescription("Get the chronological event stream history for an order")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<IReadOnlyList<OrderEventHistoryDto>>> HandleAsync(
        OrderId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var history = await sender.Send(new GetOrderHistoryQuery(id), cancellationToken);

        return TypedResults.Ok(history);
    }
}
