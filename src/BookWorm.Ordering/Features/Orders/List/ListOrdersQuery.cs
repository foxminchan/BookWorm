using BookWorm.Ordering.Domain.OrderAggregate.Specifications;

namespace BookWorm.Ordering.Features.Orders.List;

public sealed record ListOrdersQuery : IQuery<Result<IEnumerable<Order>>>;

public sealed class ListOrdersHandler(
    IReadRepository<Order> repository,
    IIdentityService identityService
) : IQueryHandler<ListOrdersQuery, Result<IEnumerable<Order>>>
{
    public async Task<Result<IEnumerable<Order>>> Handle(
        ListOrdersQuery request,
        CancellationToken cancellationToken
    )
    {
        var customerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(customerId);

        List<Order> orders;
        if (identityService.IsAdminRole())
        {
            orders = await repository.ListAsync(cancellationToken);
        }
        else
        {
            OrderFilterSpec spec = new(Guid.Parse(customerId));
            orders = await repository.ListAsync(spec, cancellationToken);
        }

        return orders;
    }
}
