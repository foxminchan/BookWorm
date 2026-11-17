using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Finance.Domain.Exceptions;
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

        var compensations = new OrderSagaCompensations(context);
        context.SetOrderStatus(order);

        logger.LogInformation(
            "[OrderOrchestrator] Order orchestration started for {OrderId} (InstanceId: {InstanceId})",
            order.Id,
            context.InstanceId
        );

        try
        {
            if (order.CanSendNotification)
            {
                compensations.AddCompensation(
                    nameof(PublishCancelOrderActivity),
                    order.ToCancelOrderCommand()
                );
            }

            await context.PublishActivityAsync(
                nameof(PublishPlaceOrderActivity),
                order.ToPlaceOrderCommand()
            );

            var events = context.CreateOrderExternalEventTasks();

            while (order is { IsTerminal: false, HasExceededMaxRetries: false })
            {
                var outcome = await context.ExecuteNextOrderStepAsync(
                    events,
                    order,
                    compensations,
                    input.MaxRetryTimeout,
                    logger
                );

                if (!outcome.IsCompleted)
                {
                    continue;
                }

                compensations.Clear();
                return outcome.Result!;
            }

            return await context.CancelDueToMaxRetriesAsync(order, compensations, logger);
        }
        catch (Exception ex) when (ex is TaskFailedException or FinanceDomainException)
        {
            logger.LogError(
                ex,
                "[OrderOrchestrator] Error in order processing saga for {OrderId}; executing compensations",
                order.Id
            );

            if (!order.IsTerminal)
            {
                order.Fail(ex.Message);
                context.SetOrderStatus(order);
            }

            await compensations.CompensateAsync();
            return order.ToOrchestrationResult();
        }
    }
}
