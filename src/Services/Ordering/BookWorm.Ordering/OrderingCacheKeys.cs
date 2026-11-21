namespace BookWorm.Ordering;

internal static class OrderingCacheKeys
{
    public const string Dashboard = "ordering:dashboard:v1";

    public static string OrderSummaryVersion(Guid orderId, long version)
    {
        return $"ordering:summary:v1:{orderId}:v{version}";
    }

    public static string OrderSummaryTerminal(Guid orderId)
    {
        return $"ordering:summary:v1:{orderId}:terminal";
    }

    public static string OrderHistoryTerminal(Guid orderId)
    {
        return $"ordering:history:v1:{orderId}:terminal";
    }
}
