using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<Result>;

public sealed class CancelOrderHandler(IRepository<Order> repository)
    : ICommandHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);

        Guard.Against.NotFound(request.OrderId, order);

        order.MarkAsCanceled();

        await repository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}
