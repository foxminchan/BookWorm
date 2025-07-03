using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Infrastructure.Helpers;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed record GetOrderQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<OrderDetailDto>;

public sealed class GetOrderHandler(IOrderRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<GetOrderQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken
    )
    {
        Order? order;
        if (claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
        {
            order = await repository.GetByIdAsync(request.Id, cancellationToken);
        }
        else
        {
            var buyerId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

            order = await repository.FirstOrDefaultAsync(
                new OrderFilterSpec(request.Id, buyerId),
                cancellationToken
            );
        }

        Guard.Against.NotFound(order, request.Id);

        return order.ToOrderDetailDto();
    }
}
