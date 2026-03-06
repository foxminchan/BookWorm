using System.Text.RegularExpressions;

namespace BookWorm.Chat.Orchestration.Conditions;

internal static partial class NegativeSentimentCondition
{
    [GeneratedRegex(
        @"\b(disappointed|frustrate[ds]?|unhappy|angry|terrible|worst|awful|hate[ds]?|poor|bad|unsatisfied)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex NegativeKeywordsRegex();

    public static bool Evaluate(List<ChatMessage>? output)
    {
        if (output is null or [])
        {
            return false;
        }

        var lastMessage = output[^1].Text;
        return !string.IsNullOrWhiteSpace(lastMessage)
            && NegativeKeywordsRegex().IsMatch(lastMessage);
    }
}
