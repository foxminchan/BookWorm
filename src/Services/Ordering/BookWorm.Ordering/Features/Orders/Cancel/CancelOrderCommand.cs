using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : IQuery<OrderDetailDto>;

public sealed class CancelOrderHandler(IOrderRepository repository)
    : IQueryHandler<CancelOrderCommand, OrderDetailDto>
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

        if (order is null)
        {
            throw new NotFoundException($"Order with id {request.OrderId} not found.");
        }

        order.MarkAsCanceled();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.ToOrderDetailDto();
    }
}
