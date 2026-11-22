using BookWorm.Chassis.EventBus.Idempotency;
using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(
    IBasketRepository repository,
    IIntegrationEventIdempotencyGuard? idempotencyGuard = null
)
{
    [WolverineHandler]
    public async Task<IntegrationEvent> Handle(
        PlaceOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        if (
            idempotencyGuard is not null
            && !await idempotencyGuard.TryAcquireAsync(
                command.OrderId,
                nameof(PlaceOrderCommandHandler),
                cancellationToken: cancellationToken
            )
        )
        {
            return new BasketDeletedCompleteIntegrationEvent(
                command.OrderId,
                command.BasketId,
                command.TotalMoney
            );
        }

        try
        {
            var basketDeleted = await repository.DeleteBasketAsync(command.BasketId.ToString());

            return basketDeleted
                ? new BasketDeletedCompleteIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.TotalMoney
                )
                : new BasketDeletedFailedIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.Email,
                    command.TotalMoney
                );
        }
        catch
        {
            if (idempotencyGuard is not null)
            {
                await idempotencyGuard.ReleaseAsync(
                    command.OrderId,
                    nameof(PlaceOrderCommandHandler),
                    CancellationToken.None
                );
            }

            throw;
        }
    }
}
