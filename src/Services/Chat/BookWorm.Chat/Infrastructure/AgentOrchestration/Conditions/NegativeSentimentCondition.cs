namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Conditions;

internal static class NegativeSentimentCondition
{
    private static readonly string[] _negativeKeywords =
    [
        "disappointed",
        "frustrate",
        "unhappy",
        "angry",
        "terrible",
        "worst",
        "awful",
        "hate",
        "poor",
        "bad",
        "unsatisfied",
    ];

    public static bool Evaluate(List<ChatMessage>? output)
    {
        if (output is null || output.Count == 0)
        {
            return false;
        }

        var lastMessage = output.LastOrDefault()?.Text.ToLowerInvariant();
        return !string.IsNullOrWhiteSpace(lastMessage)
            && _negativeKeywords.Any(lastMessage.Contains);
    }
}
