using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Features.Orders.Cancel;

internal sealed class CancelOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, OrderId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/orders/{orderId}/cancel",
                async (
                    [FromHeader(Name = Http.RequestIdHeader)]
                    [Description("The idempotency key of the order to be cancelled")]
                        string key,
                    [Description("The unique identifier of the order to be cancelled")]
                        OrderId orderId,
                    ISender sender
                ) => await HandleAsync(orderId, sender)
            )
            .ProducesPatch<OrderDetailDto>(false)
            .WithIdempotency()
            .WithTags(nameof(Order))
            .WithName(nameof(CancelOrderEndpoint))
            .WithSummary("Cancel Order")
            .WithDescription("Cancel an order")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<OrderDetailDto>> HandleAsync(
        OrderId orderId,
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(new CancelOrderCommand((Guid)orderId), cancellationToken);

        return TypedResults.Ok(result);
    }
}
