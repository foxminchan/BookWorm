using System.Text.RegularExpressions;

namespace BookWorm.Chat.Orchestration.Conditions;

internal static partial class PolicyKeywordCondition
{
    [GeneratedRegex(
        @"\b(policy|policies|returns?|refunds?|shipping|delivery|payments?|accounts?|billing|warranty|guarantee)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex PolicyKeywordsRegex();

    public static bool Evaluate(List<ChatMessage>? output)
    {
        if (output is null or [])
        {
            return false;
        }

        var lastMessage = output[^1].Text;
        return !string.IsNullOrWhiteSpace(lastMessage)
            && PolicyKeywordsRegex().IsMatch(lastMessage);
    }
}
