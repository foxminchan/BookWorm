using BookWorm.Chassis.CQRS.Command;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.Features.Orders.Complete;

public sealed record CompleteOrderCommand(Guid OrderId) : ICommand<OrderDetailDto>;

public sealed class CompleteOrderHandler(IOrderRepository repository)
    : ICommandHandler<CompleteOrderCommand, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(
        CompleteOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = await repository.FirstOrDefaultAsync(
            new OrderFilterSpec(request.OrderId, Status.New),
            cancellationToken
        );

        Guard.Against.NotFound(order, request.OrderId);

        order.MarkAsCompleted();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
