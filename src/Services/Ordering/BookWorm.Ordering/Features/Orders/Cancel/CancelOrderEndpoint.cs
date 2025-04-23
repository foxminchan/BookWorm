using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed class CancelOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/orders/{orderId:guid}/cancel",
                async (
                    [FromHeader(Name = Restful.RequestIdHeader)]
                    [Description("The idempotency key of the order to be cancelled")]
                        string key,
                    [Description("The unique identifier of the order to be cancelled")]
                        Guid orderId,
                    ISender sender
                ) => await HandleAsync(orderId, sender)
            )
            .Produces<OrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithIdempotency()
            .WithTags(nameof(Order))
            .WithName(nameof(CancelOrderEndpoint))
            .WithSummary("Cancel Order")
            .WithDescription("Cancel an order")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<OrderDetailDto>> HandleAsync(
        Guid orderId,
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await request.Send(new CancelOrderCommand(orderId), cancellationToken);

        return TypedResults.Ok(result);
    }
}
