using BookWorm.Contracts;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

public sealed class DeleteBasketCompleteCommandHandler(
    ILogger<DeleteBasketCompleteCommandHandler> logger
) : IConsumer<DeleteBasketCompleteCommand>
{
    public Task Consume(ConsumeContext<DeleteBasketCompleteCommand> context)
    {
        var message = context.Message;

        logger.LogInformation(
            "Basket deletion completed for Order {OrderId}, Amount: {TotalMoney}",
            message.OrderId,
            message.TotalMoney
        );

        return Task.CompletedTask;
    }
}

[ExcludeFromCodeCoverage]
public sealed class DeleteBasketCompleteCommandHandlerDefinition
    : ConsumerDefinition<DeleteBasketCompleteCommandHandler>
{
    public DeleteBasketCompleteCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "basket-checkout-complete");
        ConcurrentMessageLimit = 1;
    }
}
