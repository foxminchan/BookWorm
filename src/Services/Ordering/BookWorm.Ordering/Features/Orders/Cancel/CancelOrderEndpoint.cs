namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed class CancelOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/orders/{orderId:guid}/cancel",
                async ([FromIdempotencyHeader] string key, Guid orderId, ISender sender) =>
                    await HandleAsync(orderId, sender)
            )
            .Produces<OrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithIdempotency()
            .WithOpenApi()
            .WithTags(nameof(Order))
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
