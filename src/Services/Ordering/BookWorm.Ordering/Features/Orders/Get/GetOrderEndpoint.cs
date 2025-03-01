namespace BookWorm.Ordering.Features.Orders.Get;

public sealed class GetOrderEndpoint : IEndpoint<Ok<OrderDetailDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .Produces<OrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Order))
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
