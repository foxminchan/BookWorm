using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;

namespace BookWorm.Ordering.Features.Orders.Cancel;

[Transactional]
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
        var spec = claimsPrincipal.HasRole(Authorization.Roles.Admin)
            ? new OrderFilterSpec(request.OrderId, Status.New)
            : new OrderFilterSpec(
                request.OrderId,
                (Guid)claimsPrincipal[ClaimTypes.NameIdentifier].ToBuyerId()
            );

        var order = await repository.FirstOrDefaultAsync(spec, cancellationToken);

        Guard.Against.NotFound(order, request.OrderId);

        order.MarkAsCanceled();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
