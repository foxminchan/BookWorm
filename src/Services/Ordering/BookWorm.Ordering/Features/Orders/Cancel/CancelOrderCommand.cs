using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<OrderDetailDto>;

internal sealed class CancelOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CancelOrderCommand, OrderDetailDto>
{
    public async ValueTask<OrderDetailDto> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        Order? order;

        if (claimsPrincipal.HasRole(Authorization.Roles.Admin))
        {
            order = await repository.FirstOrDefaultAsync(
                new OrderFilterSpec(request.OrderId, Status.New),
                cancellationToken
            );
        }
        else
        {
            var buyerId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

            order = await repository.FirstOrDefaultAsync(
                new OrderFilterSpec(request.OrderId, buyerId),
                cancellationToken
            );
        }

        Guard.Against.NotFound(order, request.OrderId);

        order.MarkAsCanceled();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
