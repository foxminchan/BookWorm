using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.Features.Orders.Complete;

public sealed record CompleteOrderCommand(Guid OrderId) : ICommand<Result>;

public sealed class CompleteOrderHandler(IRepository<Order> repository)
    : ICommandHandler<CompleteOrderCommand, Result>
{
    public async Task<Result> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);

        Guard.Against.NotFound(request.OrderId, order);

        order.MarkAsCompleted();

        await repository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}
