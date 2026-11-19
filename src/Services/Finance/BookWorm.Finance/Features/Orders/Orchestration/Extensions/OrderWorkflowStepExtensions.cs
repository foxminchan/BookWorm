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
            OrderSagaCompensations compensations,
            TimeSpan retryTimeout,
            ILogger logger
        )
        {
            var winner = await context.WaitForNextOrderStepAsync(events, order, retryTimeout);

            var stepContext = new OrderStepContext(context, events, order, compensations, logger);
            var step = _steps.First(s => s.CanHandle(winner, events));

            return await step.ExecuteAsync(stepContext);
        }

        public async Task<OrderOrchestrationResult> CancelDueToMaxRetriesAsync(
            Order order,
            OrderSagaCompensations compensations,
            ILogger logger
        )
        {
            order.CancelDueToMaxRetries();
            context.SetOrderStatus(order);

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

            compensations.Clear();
            return order.ToOrchestrationResult();
        }
    }
}
