using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed record GetBuyerQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<BuyerDto>;

internal sealed class GetBuyerHandler(IBuyerRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<GetBuyerQuery, BuyerDto>
{
    public async ValueTask<BuyerDto> Handle(
        GetBuyerQuery request,
        CancellationToken cancellationToken
    )
    {
        var buyerId = claimsPrincipal.HasRole(Authorization.Roles.Admin)
            ? request.Id
            : claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

        var result = await repository.GetByIdAsync(buyerId, cancellationToken);

        Guard.Against.NotFound(result, buyerId);

        return result.ToBuyerDto();
    }
}
