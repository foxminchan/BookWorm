using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(IBasketRepository repository)
{
    [WolverineHandler]
    public async Task<IntegrationEvent> Handle(
        PlaceOrderCommand command,
        CancellationToken cancellationToken
    )
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
}
