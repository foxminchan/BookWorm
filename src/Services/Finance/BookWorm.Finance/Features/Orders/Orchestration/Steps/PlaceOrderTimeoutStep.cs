namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal sealed class PlaceOrderTimeoutStep : IOrderWorkflowStep
{
    public bool CanHandle(Task winner, ExternalEventTasks events)
    {
        return true;
    }

    public async Task<OrderStepOutcome> ExecuteAsync(OrderStepContext stepContext)
    {
        var order = stepContext.Order;

        if (!order.RecordTimeoutRetry())
        {
            return OrderStepOutcome.Continue;
        }

        stepContext.Logger.LogWarning(
            "[OrderOrchestrator] PlaceOrder timed out for {OrderId}; retry {Attempt}/{MaxAttempts}",
            order.Id,
            order.TimeoutRetryCount,
            order.MaxAttempts
        );

        await stepContext.OrchestrationContext.PublishActivityAsync(
            nameof(PublishPlaceOrderActivity),
            order.ToPlaceOrderCommand()
        );

        return OrderStepOutcome.Continue;
    }
}
