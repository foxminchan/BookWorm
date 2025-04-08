using BookWorm.Ordering.Helpers;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed record GetBuyerQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<BuyerDto>;

public sealed class GetBuyerHandler(
    IBuyerRepository buyerRepository,
    ClaimsPrincipal claimsPrincipal
) : IQueryHandler<GetBuyerQuery, BuyerDto>
{
    public async Task<BuyerDto> Handle(GetBuyerQuery request, CancellationToken cancellationToken)
    {
        var buyerId = claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin)
            ? request.Id
            : claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToBuyerId();

        var result = await buyerRepository.GetByIdAsync(buyerId, cancellationToken);

        Guard.Against.NotFound(result, $"Buyer with id {buyerId} not found.");

        return result.ToBuyerDto();
    }
}
