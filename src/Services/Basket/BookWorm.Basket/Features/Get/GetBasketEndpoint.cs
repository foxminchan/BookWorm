using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Basket.Features.Get;

public sealed class GetBasketEndpoint : IEndpoint<Ok<CustomerBasketDto>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                string.Empty,
                async (ISender sender, CancellationToken cancellationToken) =>
                    await HandleAsync(sender, cancellationToken)
            )
            .ProducesGet<CustomerBasketDto>(hasNotFound: true)
            .WithTags(nameof(Basket))
            .WithName(nameof(GetBasketEndpoint))
            .WithSummary("Get Basket")
            .WithDescription("Get a basket by user")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
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
