namespace BookWorm.Ordering.Features.Orders.List;

public sealed class ListOrdersEndpoint
    : IEndpoint<Ok<PagedResult<OrderDto>>, ListOrdersQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/orders",
                async ([AsParameters] ListOrdersQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<PagedResult<OrderDto>>()
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Order))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<PagedResult<OrderDto>>> HandleAsync(
        ListOrdersQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
