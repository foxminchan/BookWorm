using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

internal sealed class DeleteBasketFailedCommandHandler(
    IOrderRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task Handle(DeleteBasketFailedCommand message, CancellationToken cancellationToken)
    {
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                message.Id,
                nameof(DeleteBasketFailedCommandHandler),
                cancellationToken: cancellationToken
            )
        )
        {
            return;
        }

        var order = await repository.GetByIdAsync(message.OrderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        order.Delete();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}
