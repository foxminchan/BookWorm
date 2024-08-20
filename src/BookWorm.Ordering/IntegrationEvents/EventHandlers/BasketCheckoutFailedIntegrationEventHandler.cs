using BookWorm.Contracts;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

internal sealed class BasketCheckoutFailedIntegrationEventHandler(
    IRepository<Order> repository,
    ILogger<BasketCheckoutFailedIntegrationEventHandler> logger) : IConsumer<BasketCheckoutFailedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutFailedIntegrationEvent> context)
    {
        var @event = context.Message;

        logger.LogInformation("[{Consumer}] - Rollback order with Id: {OrderId}",
            nameof(BasketCheckoutFailedIntegrationEventHandler), @event.OrderId);

        var order = await repository.GetByIdAsync(@event.OrderId);

        Guard.Against.NotFound(@event.OrderId, order);

        await repository.DeleteAsync(order);
    }
}

internal sealed class BasketCheckoutFailedIntegrationEventHandlerDefinition
    : ConsumerDefinition<BasketCheckoutFailedIntegrationEventHandler>
{
    public BasketCheckoutFailedIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "basket-checkout-failed");
        ConcurrentMessageLimit = 1;
    }
}
