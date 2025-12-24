namespace BookWorm.Constants.Aspire;

public static class Services
{
    public const string Basket = "basket";
    public const string Rating = "rating";
    public const string Finance = "finance";
    public const string Gateway = "gateway";
    public const string Catalog = "catalog";
    public const string Ordering = "ordering";
    public const string Chatting = "chatting";
    public const string McpTools = "mcptools";
    public const string Scheduler = "scheduler";
    public const string Notification = "notification";

    public static string ToClientName(string application, string? suffix = null)
    {
        var clientName = char.ToUpperInvariant(application[0]) + application[1..];
        return string.IsNullOrWhiteSpace(suffix) ? clientName : $"{clientName} {suffix}";
    }
}
