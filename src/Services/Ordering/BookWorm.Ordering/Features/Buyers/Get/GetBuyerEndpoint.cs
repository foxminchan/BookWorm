using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed class GetBuyerEndpoint : IEndpoint<Ok<BuyerDto>, GetBuyerQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/buyers/me",
                async ([AsParameters] GetBuyerQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .ProducesGet<BuyerDto>(hasNotFound: true)
            .WithTags(nameof(Buyer))
            .WithName(nameof(GetBuyerEndpoint))
            .WithSummary("Get Buyer")
            .WithDescription("Get the current buyer's information")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<BuyerDto>> HandleAsync(
        GetBuyerQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}
