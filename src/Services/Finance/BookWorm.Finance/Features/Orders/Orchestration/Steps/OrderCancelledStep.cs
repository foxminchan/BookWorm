using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal sealed class OrderCancelledStep : IOrderWorkflowStep
{
    public bool CanHandle(Task winner, ExternalEventTasks events)
    {
        return winner == events.OrderCancelled;
    }

    public async Task<OrderStepOutcome> ExecuteAsync(OrderStepContext stepContext)
    {
        var @event = await stepContext.Events.OrderCancelled;
        var order = stepContext.Order;
        order.Cancel(
            @event.BasketId,
            new(@event.FullName, @event.Email),
            Money.Of(@event.TotalMoney)
        );
        stepContext.OrchestrationContext.SetOrderStatus(order);

        stepContext.Logger.LogInformation(
            "[OrderOrchestrator] Order orchestration cancelled for {OrderId}",
            order.Id
        );

        if (order.CanSendNotification)
        {
            await stepContext.OrchestrationContext.PublishActivityAsync(
                nameof(PublishCancelOrderActivity),
                order.ToCancelOrderCommand()
            );
        }

        return OrderStepOutcome.Complete(order.ToOrchestrationResult());
    }
}
