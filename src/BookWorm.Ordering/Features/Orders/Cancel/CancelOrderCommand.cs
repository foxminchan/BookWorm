using BookWorm.Contracts;

namespace BookWorm.Ordering.Features.Orders.Cancel;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<Result>;

public sealed class CancelOrderHandler(
    IRepository<Order> repository,
    IPublishEndpoint publishEndpoint,
    IIdentityService identityService
) : ICommandHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);

        Guard.Against.NotFound(request.OrderId, order);

        order.MarkAsCanceled();

        await repository.SaveChangesAsync(cancellationToken);

        var email = identityService.GetEmail();

        await publishEndpoint.Publish(
            new OrderCancelledIntegrationEvent(order.Id, email),
            cancellationToken
        );

        return Result.Success();
    }
}
