using Mediator;

namespace BookWorm.Basket.Features.Get;

public sealed record GetBasketQuery : IQuery<CustomerBasketDto>;

public sealed class GetBasketHandler(IBasketRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<GetBasketQuery, CustomerBasketDto>
{
    public async ValueTask<CustomerBasketDto> Handle(
        GetBasketQuery request,
        CancellationToken cancellationToken
    )
    {
        var sub = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);

        var userId = Guard.Against.NotAuthenticated(sub);

        var basket = await repository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, userId);

        return basket.ToCustomerBasketDto();
    }
}
