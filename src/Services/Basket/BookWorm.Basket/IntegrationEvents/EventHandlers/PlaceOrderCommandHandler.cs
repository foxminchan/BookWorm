using BookWorm.Contracts;
using MassTransit;
using Saunter.Attributes;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class PlaceOrderCommandHandler(
    IBasketRepository repository,
    IPublishEndpoint publishEndpoint
) : IConsumer<PlaceOrderCommand>
{
    [Channel("basket-place-order")]
    [PublishOperation(
        typeof(PlaceOrderCommand),
        OperationId = nameof(PlaceOrderCommand),
        Summary = "Delete a basket"
    )]
    public async Task Consume(ConsumeContext<PlaceOrderCommand> context)
    {
        var command = context.Message;

        var basketDeleted = await repository.DeleteBasketAsync(command.BasketId.ToString());

        if (!basketDeleted)
        {
            await PublishFailedEvent(
                command.OrderId,
                command.BasketId,
                command.Email,
                command.TotalMoney
            );
        }
        else
        {
            await PublishCompletedEvent(command.OrderId, command.BasketId, command.TotalMoney);
        }
    }

    [Channel("basket-checkout-complete")]
    [SubscribeOperation(
        typeof(BasketDeletedFailedIntegrationEvent),
        OperationId = nameof(BasketDeletedFailedIntegrationEvent)
    )]
    private async Task PublishCompletedEvent(Guid orderId, Guid basketId, decimal totalMoney)
    {
        await publishEndpoint.Publish(
            new BasketDeletedCompleteIntegrationEvent(orderId, basketId, totalMoney)
        );
    }

    [Channel("basket-checkout-failed")]
    [SubscribeOperation(
        typeof(BasketDeletedCompleteIntegrationEvent),
        OperationId = nameof(BasketDeletedCompleteIntegrationEvent)
    )]
    private async Task PublishFailedEvent(
        Guid orderId,
        Guid basketId,
        string? email,
        decimal totalMoney
    )
    {
        await publishEndpoint.Publish(
            new BasketDeletedFailedIntegrationEvent(orderId, basketId, email, totalMoney)
        );
    }
}

[ExcludeFromCodeCoverage]
public sealed class PlaceOrderCommandHandlerDefinition
    : ConsumerDefinition<PlaceOrderCommandHandler>
{
    public PlaceOrderCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "basket-place-order");
        ConcurrentMessageLimit = 1;
    }
}
