namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal interface IOrderWorkflowStep
{
    bool CanHandle(Task winner, ExternalEventTasks events);

    Task<OrderStepOutcome> ExecuteAsync(OrderStepContext stepContext);
}
