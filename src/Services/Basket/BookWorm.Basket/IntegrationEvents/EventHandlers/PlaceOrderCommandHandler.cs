using BookWorm.Contracts;
using MassTransit;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

public sealed class PlaceOrderCommandHandler(IBasketRepository repository)
    : IConsumer<PlaceOrderCommand>
{
    public async Task Consume(ConsumeContext<PlaceOrderCommand> context)
    {
        var command = context.Message;

        var basketDeleted = await repository.DeleteBasketAsync(command.BasketId.ToString());

        if (!basketDeleted)
        {
            await context.Publish(
                new BasketDeletedFailedIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.Email,
                    command.TotalMoney
                ),
                context.CancellationToken
            );
        }
        else
        {
            await context.Publish(
                new BasketDeletedCompleteIntegrationEvent(
                    command.OrderId,
                    command.BasketId,
                    command.TotalMoney
                ),
                context.CancellationToken
            );
        }
    }
}

[ExcludeFromCodeCoverage]
public sealed class PlaceOrderCommandHandlerDefinition
    : ConsumerDefinition<PlaceOrderCommandHandler>
{
    public PlaceOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "basket-place-order");
        ConcurrentMessageLimit = 8;
    }
}
