using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Features.Orders.Complete;

internal sealed class CompleteOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, OrderId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/orders/{orderId}/complete",
                async (
                    [FromHeader(Name = Http.RequestIdHeader)]
                    [Description("The idempotency key of the order to be completed")]
                        string key,
                    [Description("The unique identifier of the order to be completed")]
                        OrderId orderId,
                    ISender sender
                ) => await HandleAsync(orderId, sender)
            )
            .ProducesPatch<OrderDetailDto>(false)
            .WithIdempotency()
            .WithTags(nameof(Order))
            .WithName(nameof(CompleteOrderEndpoint))
            .WithSummary("Complete Order")
            .WithDescription("Complete an order")
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
        var result = await request.Send(new CompleteOrderCommand((Guid)orderId), cancellationToken);

        return TypedResults.Ok(result);
    }
}
