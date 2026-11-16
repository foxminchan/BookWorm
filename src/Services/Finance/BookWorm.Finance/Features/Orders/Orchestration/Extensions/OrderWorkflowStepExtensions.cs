using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Extensions;

internal static class OrderWorkflowStepExtensions
{
    private static readonly IReadOnlyList<IOrderWorkflowStep> _steps =
    [
        new BasketDeletedCompleteStep(),
        new BasketDeletedFailedStep(),
        new OrderCompletedStep(),
        new OrderCancelledStep(),
        new PlaceOrderTimeoutStep(),
    ];

    extension(TaskOrchestrationContext context)
    {
        public async Task<OrderStepOutcome> ExecuteNextOrderStepAsync(
            ExternalEventTasks events,
            Order order,
            TimeSpan retryTimeout,
            ILogger logger
        )
        {
            var winner = await context.WaitForNextOrderStepAsync(events, order, retryTimeout);

            var stepContext = new OrderStepContext(context, events, order, logger);
            var step = _steps.First(s => s.CanHandle(winner, events));

            return await step.ExecuteAsync(stepContext);
        }

        public async Task<OrderOrchestrationResult> CancelDueToMaxRetriesAsync(
            Order order,
            ILogger logger
        )
        {
            order.CancelDueToMaxRetries();

            logger.LogWarning(
                "[OrderOrchestrator] Max retries ({MaxAttempts}) exceeded for {OrderId}; cancelling order",
                order.MaxAttempts,
                order.Id
            );

            if (order.CanSendNotification)
            {
                await context.PublishActivityAsync(
                    nameof(PublishCancelOrderActivity),
                    order.ToCancelOrderCommand()
                );
            }

            return order.ToOrchestrationResult();
        }
    }
}
