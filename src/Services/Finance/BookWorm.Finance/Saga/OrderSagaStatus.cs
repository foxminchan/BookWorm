using System.ComponentModel;

namespace BookWorm.Finance.Saga;

internal enum OrderSagaStatus : byte
{
    [Description("Order Placed")]
    Placed = 0,

    [Description("Basket Deletion Failed")]
    BasketDeletionFailed = 1,

    [Description("Order Completed")]
    Completed = 2,

    [Description("Order Cancelled")]
    Cancelled = 3,
}
