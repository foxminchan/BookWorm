using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.Events;
using MassTransit;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<Result>;

public sealed class CancelOrderHandler(IRepository<Order> repository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);

        Guard.Against.NotFound(request.OrderId, order);

        order.MarkAsCanceled();

        await repository.UpdateAsync(order, cancellationToken);

        await publishEndpoint.Publish(new OrderCancelledIntegrationEvent(order.Id), cancellationToken);

        return Result.Success();
    }
}
