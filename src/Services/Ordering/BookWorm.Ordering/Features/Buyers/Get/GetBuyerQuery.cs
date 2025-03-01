using System.ComponentModel;
using BookWorm.Ordering.Helpers;
using BookWorm.ServiceDefaults.Keycloak;

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
        Guid buyerId;
        if (claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
        {
            buyerId = request.Id;
        }
        else
        {
            buyerId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToBuyerId();
        }

        var result = await buyerRepository.GetByIdAsync(buyerId, cancellationToken);

        if (result is null)
        {
            throw new NotFoundException($"Buyer with id {buyerId} not found");
        }

        return result.ToBuyerDto();
    }
}
