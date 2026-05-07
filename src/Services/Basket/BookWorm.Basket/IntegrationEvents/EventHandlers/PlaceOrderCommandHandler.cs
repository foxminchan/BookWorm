using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(IBasketRepository repository, IMessageBus bus)
{
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
