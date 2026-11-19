using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;

namespace BookWorm.Ordering.Features.Orders.Complete;

[Transactional]
public sealed record CompleteOrderCommand(Guid OrderId) : ICommand<OrderDetailDto>;

internal sealed class CompleteOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : ICommandHandler<CompleteOrderCommand, OrderDetailDto>
{
    public async ValueTask<OrderDetailDto> Handle(
        CompleteOrderCommand request,
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

        order.MarkAsCompleted();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
