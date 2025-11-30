namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Conditions;

internal static class PolicyKeywordCondition
{
    private static readonly string[] _policyKeywords =
    [
        "policy",
        "return",
        "refund",
        "shipping",
        "delivery",
        "payment",
        "account",
        "billing",
        "warranty",
        "guarantee",
    ];

    public static bool Evaluate(List<ChatMessage>? output)
    {
        if (output is null || output.Count == 0)
        {
            return false;
        }

        var lastMessage = output.LastOrDefault()?.Text.ToLowerInvariant();
        return !string.IsNullOrWhiteSpace(lastMessage) && _policyKeywords.Any(lastMessage.Contains);
    }
}
