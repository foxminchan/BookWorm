﻿using BookWorm.Ordering.Infrastructure.Helpers;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed record GetBuyerQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<BuyerDto>;

public sealed class GetBuyerHandler(
    IBuyerRepository buyerRepository,
    ClaimsPrincipal claimsPrincipal
) : IQueryHandler<GetBuyerQuery, BuyerDto>
{
    public async ValueTask<BuyerDto> Handle(
        GetBuyerQuery request,
        CancellationToken cancellationToken
    )
    {
        var buyerId = claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin)
            ? request.Id
            : claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

        var result = await buyerRepository.GetByIdAsync(buyerId, cancellationToken);

        Guard.Against.NotFound(result, buyerId);

        return result.ToBuyerDto();
    }
}
