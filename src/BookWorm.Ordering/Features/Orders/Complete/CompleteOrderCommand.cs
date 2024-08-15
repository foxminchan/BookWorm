using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.IntegrationEvents.Events;
using BookWorm.Shared.Identity;
using MassTransit;

namespace BookWorm.Ordering.Features.Orders.Complete;

public sealed record CompleteOrderCommand(Guid OrderId) : ICommand<Result>;

public sealed class CompleteOrderHandler(
    IRepository<Order> repository,
    IPublishEndpoint publishEndpoint,
    IIdentityService identityService) : ICommandHandler<CompleteOrderCommand, Result>
{
    public async Task<Result> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);

        Guard.Against.NotFound(request.OrderId, order);

        order.MarkAsCompleted();

        await repository.UpdateAsync(order, cancellationToken);

        var email = identityService.GetEmail();

        await publishEndpoint.Publish(new OrderCompletedIntegrationEvent(order.Id, email), cancellationToken);

        return Result.Success();
    }
}
