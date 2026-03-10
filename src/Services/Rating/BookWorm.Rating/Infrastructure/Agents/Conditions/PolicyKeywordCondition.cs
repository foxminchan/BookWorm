using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Agents.Conditions;

internal static partial class PolicyKeywordCondition
{
    [GeneratedRegex(
        @"\b(return\s+policy|refund\s+policy|shipping\s+policy|return\s+procedure"
            + @"|how\s+to\s+return|how\s+do\s+i\s+return|can\s+i\s+return"
            + @"|account\s+(issue|problem|setting|management|access)"
            + "|refunds?|shipping|delivery|billing|warranty|guarantee"
            + @"|payments?\s+(method|option|issue|problem)"
            + @"|privacy\s+policy|terms\s+of\s+(service|use)|cookie\s+policy"
            + @"|policies)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex PolicyPhrasesRegex();

    public static bool Evaluate(List<ChatMessage>? output)
    {
        if (output is null or [])
        {
            return false;
        }

        var lastMessage = output[^1].Text;
        return !string.IsNullOrWhiteSpace(lastMessage) && PolicyPhrasesRegex().IsMatch(lastMessage);
    }
}
