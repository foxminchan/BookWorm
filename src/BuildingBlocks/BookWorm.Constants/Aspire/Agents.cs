namespace BookWorm.Constants.Aspire;

public static class Agents
{
    private const string Suffix = "agent";

    public static readonly string Book = $"{nameof(Book).ToLowerInvariant()}-{Suffix}";
    public static readonly string Rating = $"{nameof(Rating).ToLowerInvariant()}-{Suffix}";
    public static readonly string Language = $"{nameof(Language).ToLowerInvariant()}-{Suffix}";
    public static readonly string Summarize = $"{nameof(Summarize).ToLowerInvariant()}-{Suffix}";
    public static readonly string Sentiment = $"{nameof(Sentiment).ToLowerInvariant()}-{Suffix}";
}
