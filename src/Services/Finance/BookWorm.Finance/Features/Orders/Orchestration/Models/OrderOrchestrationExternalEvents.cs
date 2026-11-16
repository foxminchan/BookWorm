using BookWorm.Contracts;

namespace BookWorm.Finance.Features.Orders.Orchestration.Models;

internal static class OrderOrchestrationExternalEvents
{
    public const string BasketDeletedComplete = nameof(BasketDeletedCompleteIntegrationEvent);
    public const string BasketDeletedFailed = nameof(BasketDeletedFailedIntegrationEvent);
    public const string OrderCompleted = nameof(OrderStatusChangedToCompleteIntegrationEvent);
    public const string OrderCancelled = nameof(OrderStatusChangedToCancelIntegrationEvent);
}
