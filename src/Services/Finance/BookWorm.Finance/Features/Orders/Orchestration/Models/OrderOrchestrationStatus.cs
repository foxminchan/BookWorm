using System.ComponentModel;

namespace BookWorm.Finance.Features.Orders.Orchestration.Models;

public enum OrderOrchestrationStatus : byte
{
    [Description("Order Placed")]
    Placed = 0,

    [Description("Basket Deletion Failed")]
    BasketDeletionFailed = 1,

    [Description("Order Completed")]
    Completed = 2,

    [Description("Order Cancelled")]
    Cancelled = 3,

    [Description("Order Failed")]
    Failed = 4,
}
