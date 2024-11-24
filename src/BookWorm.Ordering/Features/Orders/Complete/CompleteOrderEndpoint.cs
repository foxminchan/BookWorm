namespace BookWorm.Ordering.Features.Orders.Complete;

public sealed class CompleteOrderEndpoint : IEndpoint<Ok, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/orders/{orderId:guid}/complete",
                async ([FromIdempotencyHeader] string key, Guid orderId, ISender sender) =>
                    await HandleAsync(orderId, sender)
            )
            .AddEndpointFilter<IdempotencyFilter>()
            .Produces<Ok>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Order))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new CompleteOrderCommand(id), cancellationToken);

        return TypedResults.Ok();
    }
}
