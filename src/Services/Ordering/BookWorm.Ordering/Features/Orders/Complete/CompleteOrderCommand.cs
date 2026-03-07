using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Orders.Complete;

public sealed record CompleteOrderCommand(Guid OrderId) : ICommand<OrderDetailDto>;

public sealed class CompleteOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CompleteOrderCommand, OrderDetailDto>
{
    public async ValueTask<OrderDetailDto> Handle(
        CompleteOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        Order? order;

        if (claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
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

        order.MarkAsCompleted();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
