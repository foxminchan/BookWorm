using BookWorm.Contracts;
using Wolverine;
using Wolverine.Attributes;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(IBasketRepository repository, IMessageBus bus)
{
    [WolverineHandler]
    public async Task Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var basketDeleted = await repository.DeleteBasketAsync(command.BasketId.ToString());

        if (!basketDeleted)
        {
            await bus.PublishAsync(
                new BasketDeletedFailedIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.Email,
                    command.TotalMoney
                )
            );
        }
        else
        {
            await bus.PublishAsync(
                new BasketDeletedCompleteIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.TotalMoney
                )
            );
        }
    }
}
