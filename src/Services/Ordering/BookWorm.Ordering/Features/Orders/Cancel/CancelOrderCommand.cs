using BookWorm.Chassis.CQRS.Command;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<OrderDetailDto>;

public sealed class CancelOrderHandler(IOrderRepository repository)
    : ICommandHandler<CancelOrderCommand, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = await repository.FirstOrDefaultAsync(
            new OrderFilterSpec(request.OrderId, Status.New),
            cancellationToken
        );

        Guard.Against.NotFound(order, request.OrderId);

        order.MarkAsCanceled();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
