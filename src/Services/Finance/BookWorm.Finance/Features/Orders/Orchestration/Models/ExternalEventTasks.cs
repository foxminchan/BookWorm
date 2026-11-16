using BookWorm.Contracts;

namespace BookWorm.Finance.Features.Orders.Orchestration.Models;

internal sealed record ExternalEventTasks(
    Task<BasketDeletedCompleteIntegrationEvent> BasketDeletedComplete,
    Task<BasketDeletedFailedIntegrationEvent> BasketDeletedFailed,
    Task<OrderStatusChangedToCompleteIntegrationEvent> OrderCompleted,
    Task<OrderStatusChangedToCancelIntegrationEvent> OrderCancelled
);
