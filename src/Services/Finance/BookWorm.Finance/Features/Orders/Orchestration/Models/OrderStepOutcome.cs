namespace BookWorm.Finance.Features.Orders.Orchestration.Models;

internal readonly record struct OrderStepOutcome(OrderOrchestrationResult? Result)
{
    public static OrderStepOutcome Continue => default;

    public bool IsCompleted => Result is not null;

    public static OrderStepOutcome Complete(OrderOrchestrationResult result)
    {
        return new(result);
    }
}
