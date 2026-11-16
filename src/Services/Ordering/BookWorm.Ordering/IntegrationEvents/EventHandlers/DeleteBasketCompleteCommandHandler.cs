using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

internal sealed partial class DeleteBasketCompleteCommandHandler(
    ILogger<DeleteBasketCompleteCommandHandler> logger
)
{
    [WolverineHandler]
    public Task Handle(DeleteBasketCompleteCommand message, CancellationToken cancellationToken)
    {
        LogBasketDeletionCompleted(logger, message.OrderId, message.TotalMoney);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 0,
        EventName = nameof(LogBasketDeletionCompleted),
        Level = LogLevel.Information,
        Message = "Basket deletion completed for Order {OrderId}, Amount: {TotalMoney}"
    )]
    private static partial void LogBasketDeletionCompleted(
        ILogger logger,
        [SensitiveData] Guid orderId,
        decimal totalMoney
    );
}
