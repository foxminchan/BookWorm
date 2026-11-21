using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;

namespace BookWorm.Ordering.Features.Orders;

internal static class OrderOwnershipExtensions
{
    extension(IOrderRepository repository)
    {
        public async ValueTask EnsureOrderAccessAsync(
            OrderId id,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken
        )
        {
            if (claimsPrincipal.HasRole(Authorization.Roles.Admin))
            {
                return;
            }

            var orderId = (Guid)id;
            var buyerId = claimsPrincipal[ClaimTypes.NameIdentifier].ToBuyerId();
            var exists = await repository.AnyAsync(
                new OrderFilterSpec(orderId, (Guid)buyerId),
                cancellationToken
            );

            if (!exists)
            {
                throw new NotFoundException($"Order with id {orderId} not found.");
            }
        }
    }
}
