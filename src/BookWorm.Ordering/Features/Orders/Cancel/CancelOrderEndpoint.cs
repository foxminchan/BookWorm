namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed class CancelOrderEndpoint : IEndpoint<Ok, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/orders/{orderId:guid}/cancel",
                async (
                    [FromIdempotencyHeader] string key,
                    Guid orderId,
                    ISender sender) => await HandleAsync(orderId, sender))
            .AddEndpointFilter<IdempotencyFilter>()
            .Produces<Ok>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Order))
            .WithName("Cancel Order")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok> HandleAsync(Guid id, ISender sender, CancellationToken cancellationToken = default)
    {
        await sender.Send(new CancelOrderCommand(id), cancellationToken);

        return TypedResults.Ok();
    }
}
