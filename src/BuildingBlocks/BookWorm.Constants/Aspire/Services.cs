namespace BookWorm.Constants.Aspire;

public static class Services
{
    public static readonly string Basket = nameof(Basket).ToLowerInvariant();
    public static readonly string Rating = nameof(Rating).ToLowerInvariant();
    public static readonly string Finance = nameof(Finance).ToLowerInvariant();
    public static readonly string Gateway = nameof(Gateway).ToLowerInvariant();
    public static readonly string Catalog = nameof(Catalog).ToLowerInvariant();
    public static readonly string Ordering = nameof(Ordering).ToLowerInvariant();
    public static readonly string Chatting = nameof(Chatting).ToLowerInvariant();
    public static readonly string McpTools = nameof(McpTools).ToLowerInvariant();
    public static readonly string Scheduler = nameof(Scheduler).ToLowerInvariant();
    public static readonly string Notification = nameof(Notification).ToLowerInvariant();

    public static string ToClientName(string application, string? suffix = null)
    {
        var clientName = char.ToUpperInvariant(application[0]) + application[1..];
        return string.IsNullOrWhiteSpace(suffix) ? clientName : $"{clientName} {suffix}";
    }
}
