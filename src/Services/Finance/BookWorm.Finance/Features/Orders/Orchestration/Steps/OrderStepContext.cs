using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Finance.Features.Orders.Orchestration.Steps;

internal sealed record OrderStepContext(
    TaskOrchestrationContext OrchestrationContext,
    ExternalEventTasks Events,
    Order Order,
    ILogger Logger
);
