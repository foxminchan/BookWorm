﻿using BookWorm.Constants.Other;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed class GetBuyerEndpoint
    : IEndpoint<Ok<BuyerDto>, GetBuyerQuery, ISender, IFeatureManager>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/buyers/me",
                async (
                    [AsParameters] GetBuyerQuery query,
                    ISender sender,
                    IFeatureManager featureManager
                ) => await HandleAsync(query, sender, featureManager)
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
        IFeatureManager featureManager,
        CancellationToken cancellationToken = default
    )
    {
        var enabledAddress = await featureManager.IsEnabledAsync(
            nameof(FeatureFlags.EnableAddress)
        );

        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(enabledAddress ? result : result with { Address = null });
    }
}
