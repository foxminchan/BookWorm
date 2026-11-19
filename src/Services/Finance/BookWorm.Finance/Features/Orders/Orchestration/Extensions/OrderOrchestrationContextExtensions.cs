using BookWorm.Contracts;
using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Extensions;

internal static class OrderOrchestrationContextExtensions
{
    private static readonly TaskOptions _activityRetryOptions = TaskOptions.FromRetryPolicy(
        new(3, TimeSpan.FromSeconds(5), 2.0)
    );

    extension(TaskOrchestrationContext context)
    {
        public Task PublishActivityAsync(string activityName, object command)
        {
            return context.CallActivityAsync(activityName, command, _activityRetryOptions);
        }

        public void SetOrderStatus(Order order)
        {
            context.SetCustomStatus(
                new
                {
                    OrderId = order.Id,
                    Status = order.Status.ToString(),
                    order.BasketDeleted,
                    order.TimeoutRetryCount,
                    order.FailureReason,
                }
            );
        }

        public ExternalEventTasks CreateOrderExternalEventTasks()
        {
            return new(
                context.WaitForExternalEvent<BasketDeletedCompleteIntegrationEvent>(
                    OrderOrchestrationExternalEvents.BasketDeletedComplete
                ),
                context.WaitForExternalEvent<BasketDeletedFailedIntegrationEvent>(
                    OrderOrchestrationExternalEvents.BasketDeletedFailed
                ),
                context.WaitForExternalEvent<OrderStatusChangedToCompleteIntegrationEvent>(
                    OrderOrchestrationExternalEvents.OrderCompleted
                ),
                context.WaitForExternalEvent<OrderStatusChangedToCancelIntegrationEvent>(
                    OrderOrchestrationExternalEvents.OrderCancelled
                )
            );
        }

        public async Task<Task> WaitForNextOrderStepAsync(
            ExternalEventTasks events,
            Order order,
            TimeSpan retryTimeout
        )
        {
            using var timeoutCts = new CancellationTokenSource();
            var timeoutTask = context.CreateTimer(retryTimeout, timeoutCts.Token);

            var pendingTasks = new List<Task>
            {
                events.OrderCompleted,
                events.OrderCancelled,
                timeoutTask,
            };

            if (!order.BasketDeleted)
            {
                pendingTasks.Add(events.BasketDeletedComplete);
                pendingTasks.Add(events.BasketDeletedFailed);
            }

            var winner = await Task.WhenAny(pendingTasks);
            if (winner != timeoutTask)
            {
                await timeoutCts.CancelAsync();
            }

            return winner;
        }
    }
}
