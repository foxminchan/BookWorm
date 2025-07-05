namespace BookWorm.Rating.Features;

public sealed record SummarizeResult(string Summary)
{
    public static implicit operator string(SummarizeResult result)
    {
        return result.Summary;
    }

    public static implicit operator SummarizeResult(string summary)
    {
        return new(summary);
    }
}
