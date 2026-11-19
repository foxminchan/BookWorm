using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal sealed class BasketDeletedFailedStep : IOrderWorkflowStep
{
    public bool CanHandle(Task winner, ExternalEventTasks events)
    {
        return winner == events.BasketDeletedFailed;
    }

    public async Task<OrderStepOutcome> ExecuteAsync(OrderStepContext stepContext)
    {
        var @event = await stepContext.Events.BasketDeletedFailed;
        var order = stepContext.Order;
        order.FailBasketDeletion(@event.BasketId, @event.Email, Money.Of(@event.TotalMoney));
        stepContext.OrchestrationContext.SetOrderStatus(order);

        stepContext.Logger.LogError(
            "[OrderOrchestrator] Basket deletion failed for {OrderId}",
            order.Id
        );

        await stepContext.OrchestrationContext.PublishActivityAsync(
            nameof(PublishDeleteBasketFailedActivity),
            order.ToDeleteBasketFailedCommand()
        );

        return OrderStepOutcome.Complete(order.ToOrchestrationResult());
    }
}
