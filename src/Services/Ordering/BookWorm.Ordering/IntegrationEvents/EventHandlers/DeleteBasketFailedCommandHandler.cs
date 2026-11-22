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
        await idempotencyGuard.ExecuteAsync(
            message.Id,
            nameof(DeleteBasketFailedCommandHandler),
            async ct =>
            {
                var order = await repository.GetByIdAsync(message.OrderId, ct);

                if (order is null)
                {
                    return;
                }

                order.Delete();

                await repository.UnitOfWork.SaveEntitiesAsync(ct);
            },
            cancellationToken: cancellationToken
        );
    }
}
