using BookWorm.Contracts;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

internal sealed class PlaceOrderCommandHandler(IBasketRepository repository)
{
    public async Task Handle(
        PlaceOrderCommand command,
        IMessageContext context,
        CancellationToken cancellationToken
    )
    {
        var basketDeleted = await repository.DeleteBasketAsync(command.BasketId.ToString());

        if (!basketDeleted)
        {
            await context.PublishAsync(
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
            await context.PublishAsync(
                new BasketDeletedCompleteIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.TotalMoney
                )
            );
        }
    }
}
