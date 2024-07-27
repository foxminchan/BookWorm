using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Infrastructure.Identity;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed record GetBuyerQuery(Guid BuyerId) : IQuery<Result<Buyer>>;

public sealed class GetBuyerHandler(IReadRepository<Buyer> repository, IIdentityService identityService)
    : IQueryHandler<GetBuyerQuery, Result<Buyer>>
{
    public async Task<Result<Buyer>> Handle(GetBuyerQuery request, CancellationToken cancellationToken)
    {
        Buyer? buyer;
        if (identityService.IsAdminRole())
        {
            buyer = await repository.GetByIdAsync(request.BuyerId, cancellationToken);

            Guard.Against.NotFound(request.BuyerId, buyer);
        }
        else
        {
            var customerId = identityService.GetUserIdentity();
            Guard.Against.NullOrEmpty(customerId);

            buyer = await repository.GetByIdAsync(Guid.Parse(customerId), cancellationToken);
            Guard.Against.NotFound(Guid.Parse(customerId), buyer);
        }

        return buyer;
    }
}
