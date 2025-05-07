namespace BookWorm.Ordering.Features.Orders.Get;

public sealed class GetOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id:guid}",
                async (
                    [Description("The unique identifier of the order to be retrieved")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesGet<OrderDetailDto>(hasNotFound: true)
            .WithTags(nameof(Order))
            .WithName(nameof(GetOrderEndpoint))
            .WithSummary("Get Order")
            .WithDescription("Get an order if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<OrderDetailDto>> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var order = await sender.Send(new GetOrderQuery(id), cancellationToken);

        return TypedResults.Ok(order);
    }
}
