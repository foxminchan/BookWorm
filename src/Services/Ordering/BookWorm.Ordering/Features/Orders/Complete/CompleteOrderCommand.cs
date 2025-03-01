using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.Features.Orders.Complete;

public sealed record CompleteOrderCommand(Guid OrderId) : IQuery<OrderDetailDto>;

public sealed class CompleteOrderHandler(IOrderRepository repository)
    : IQueryHandler<CompleteOrderCommand, OrderDetailDto>
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

        if (order is null)
        {
            throw new NotFoundException($"Order with id {request.OrderId} not found.");
        }

        order.MarkAsCompleted();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
