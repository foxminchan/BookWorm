namespace BookWorm.Notification.Infrastructure.Table;

public static class TablePartition
{
    public static readonly string Pending = nameof(Pending).ToLowerInvariant();
    public static readonly string Processed = nameof(Processed).ToLowerInvariant();
}
