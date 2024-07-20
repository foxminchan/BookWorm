using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Domain.BuyerAggregate.Specifications;
using BookWorm.Ordering.Infrastructure.Identity;

namespace BookWorm.Ordering.Features.Get;

public sealed record GetOrderQuery(Guid OrderId) : ICommand<Result<Buyer>>;

public sealed class GetOrderHandler(IReadRepository<Buyer> repository, IIdentityService identityService)
    : ICommandHandler<GetOrderQuery, Result<Buyer>>
{
    public async Task<Result<Buyer>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var customerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(customerId);

        BuyerFilterSpec spec = new(Guid.Parse(customerId), request.OrderId);

        var buyer = await repository.FirstOrDefaultAsync(spec, cancellationToken);

        Guard.Against.NotFound(customerId, buyer);

        return buyer;
    }
}
