namespace BookWorm.Finance.Saga;

internal enum OrderSagaStatus : byte
{
    Placed = 0,
    BasketDeletionFailed = 1,
    Completed = 2,
    Cancelled = 3,
}
