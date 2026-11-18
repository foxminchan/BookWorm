using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal sealed class BasketDeletedCompleteStep : IOrderWorkflowStep
{
    public bool CanHandle(Task winner, ExternalEventTasks events)
    {
        return winner == events.BasketDeletedComplete;
    }

    public async Task<OrderStepOutcome> ExecuteAsync(OrderStepContext stepContext)
    {
        var @event = await stepContext.Events.BasketDeletedComplete;
        var order = stepContext.Order;
        order.MarkBasketDeleted(@event.BasketId, Money.Of(@event.TotalMoney));
        stepContext.OrchestrationContext.SetOrderStatus(order);

        // Register compensation before publishing the forward basket-completion command
        stepContext.Compensations.AddCompensation(
            nameof(PublishDeleteBasketFailedActivity),
            order.ToDeleteBasketFailedCommand()
        );

        await stepContext.OrchestrationContext.PublishActivityAsync(
            nameof(PublishDeleteBasketCompleteActivity),
            order.ToDeleteBasketCompleteCommand()
        );

        return OrderStepOutcome.Continue;
    }
}
