using BookWorm.Basket.IntegrationEvents.Events;

namespace BookWorm.Basket.IntegrationEvents.EventHandlers;

public sealed class OrderCreatedIntegrationEventHandler(
    IRedisService redisService,
    IPublishEndpoint publishEndpoint) : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var basketId = context.Message.BasketId.ToString();
        var basket = await redisService.HashGetAsync<Domain.Basket?>(nameof(Basket), basketId);

        if (basket is null)
        {
            await PublishBasketCheckoutFailed(context.Message.OrderId);
            return;
        }

        try
        {
            await redisService.HashRemoveAsync(nameof(Basket), basketId);
        }
        catch (Exception)
        {
            await PublishBasketCheckoutFailed(context.Message.OrderId);
        }
    }

    private async Task PublishBasketCheckoutFailed(Guid orderId)
    {
        await publishEndpoint.Publish(new BasketCheckoutFailedIntegrationEvent(orderId));
    }
}
