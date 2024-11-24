using BookWorm.Contracts;
using BasketModel = BookWorm.Basket.Domain.Basket;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

public sealed class OrderCreatedIntegrationEventHandler(
    IRedisService redisService,
    ILogger<OrderCreatedIntegrationEventHandler> logger,
    IPublishEndpoint publishEndpoint
) : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation(
            "[{Consumer}] - Removing basket for order {OrderId}",
            nameof(OrderCreatedIntegrationEventHandler),
            @event.OrderId
        );

        var basketId = context.Message.BasketId.ToString();
        var basket = await redisService.HashGetAsync<BasketModel?>(nameof(Basket), basketId);

        if (basket is null)
        {
            await PublishBasketCheckoutFailed(@event.OrderId);
            return;
        }

        try
        {
            await redisService.HashRemoveAsync(nameof(Basket), basketId);
        }
        catch (Exception)
        {
            await PublishBasketCheckoutFailed(@event.OrderId);
        }
    }

    private async Task PublishBasketCheckoutFailed(Guid orderId)
    {
        await publishEndpoint.Publish(new BasketCheckoutFailedIntegrationEvent(orderId));
    }
}

internal sealed class OrderCreatedIntegrationEventHandlerDefinition
    : ConsumerDefinition<OrderCreatedIntegrationEventHandler>
{
    public OrderCreatedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "order-created");
        ConcurrentMessageLimit = 1;
    }
}
