using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;
using Microsoft.Azure.Functions.Worker;

namespace BookWorm.Finance.Features.Orders.Orchestration;

public sealed class OrderOrchestrator
{
    [Function(nameof(OrderOrchestrator))]
    public async Task<OrderOrchestrationResult> RunAsync(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        OrderOrchestrationInput input
    )
    {
        ArgumentNullException.ThrowIfNull(input);

        var logger = context.CreateReplaySafeLogger<OrderOrchestrator>();
        var order = Order.Place(
            input.OrderId,
            input.BasketId,
            new(input.FullName, input.Email),
            Money.Of(input.TotalMoney),
            input.MaxAttempts,
            context.CurrentUtcDateTime
        );

        logger.LogInformation(
            "[OrderOrchestrator] Order orchestration started for {OrderId}",
            order.Id
        );
        await context.PublishActivityAsync(
            nameof(PublishPlaceOrderActivity),
            order.ToPlaceOrderCommand()
        );

        var events = context.CreateOrderExternalEventTasks();

        while (!order.HasExceededMaxRetries)
        {
            var outcome = await context.ExecuteNextOrderStepAsync(
                events,
                order,
                input.MaxRetryTimeout,
                logger
            );

            if (outcome.IsCompleted)
            {
                return outcome.Result!;
            }
        }

        return await context.CancelDueToMaxRetriesAsync(order, logger);
    }
}
