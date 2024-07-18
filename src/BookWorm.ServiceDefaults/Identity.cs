namespace BookWorm.ServiceDefaults;

public sealed class Identity
{
    public Dictionary<string, string> Scopes = [];
    public string Url { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
