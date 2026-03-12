using BookWorm.Contracts;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

public sealed class DeleteBasketCompleteCommandHandler(
    ILogger<DeleteBasketCompleteCommandHandler> logger
)
{
    public Task Handle(DeleteBasketCompleteCommand message)
    {
        logger.LogInformation(
            "Basket deletion completed for Order {OrderId}, Amount: {TotalMoney}",
            message.OrderId,
            message.TotalMoney
        );

        return Task.CompletedTask;
    }
}
