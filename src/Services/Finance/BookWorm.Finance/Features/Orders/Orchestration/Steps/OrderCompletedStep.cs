using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal sealed class OrderCompletedStep : IOrderWorkflowStep
{
    public bool CanHandle(Task winner, ExternalEventTasks events)
    {
        return winner == events.OrderCompleted;
    }

    public async Task<OrderStepOutcome> ExecuteAsync(OrderStepContext stepContext)
    {
        var @event = await stepContext.Events.OrderCompleted;
        var order = stepContext.Order;
        order.Complete(
            @event.BasketId,
            new(@event.FullName, @event.Email),
            Money.Of(@event.TotalMoney)
        );
        stepContext.OrchestrationContext.SetOrderStatus(order);

        stepContext.Logger.LogInformation(
            "[OrderOrchestrator] Order orchestration completed for {OrderId}",
            order.Id
        );

        if (order.CanSendNotification)
        {
            await stepContext.OrchestrationContext.PublishActivityAsync(
                nameof(PublishCompleteOrderActivity),
                order.ToCompleteOrderCommand()
            );
        }

        return OrderStepOutcome.Complete(order.ToOrchestrationResult());
    }
}
