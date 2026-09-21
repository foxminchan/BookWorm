using Mediator;

namespace BookWorm.Ordering.Features.Orders.Get;

internal sealed class GetOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, OrderId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id}",
                async (
                    [Description("The unique identifier of the order to be retrieved")] OrderId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesGet<OrderDetailDto>(hasNotFound: true)
            .WithTags(nameof(Order))
            .WithName(nameof(GetOrderEndpoint))
            .WithSummary("Get Order")
            .WithDescription("Get an order if it exists")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<OrderDetailDto>> HandleAsync(
        OrderId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var order = await sender.Send(new GetOrderQuery(id), cancellationToken);

        return TypedResults.Ok(order);
    }
}
