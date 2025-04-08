namespace BookWorm.Basket.Features.Get;

public sealed class GetBasketEndpoint : IEndpoint<Ok<CustomerBasketDto>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/baskets",
                async (ISender sender, CancellationToken cancellationToken) =>
                    await HandleAsync(sender, cancellationToken)
            )
            .Produces<CustomerBasketDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Basket))
            .WithName(nameof(GetBasketEndpoint))
            .WithSummary("Get Basket")
            .WithDescription("Get a basket by user")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<CustomerBasketDto>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new GetBasketQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
}
