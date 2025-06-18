using BookWorm.Contracts;
using Saunter.Attributes;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class DeleteBasketCompleteCommandHandler(
    ILogger<DeleteBasketCompleteCommandHandler> logger
) : IConsumer<DeleteBasketCompleteCommand>
{
    [Channel("basket-checkout-complete")]
    [PublishOperation(
        typeof(DeleteBasketCompleteCommand),
        OperationId = nameof(DeleteBasketCompleteCommand),
        Summary = "Delete basket complete",
        Description = "Represents an integration event that is published when reverse basket is completed"
    )]
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
