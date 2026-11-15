using System.Text.RegularExpressions;
using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

namespace BookWorm.Chat.Domain.Services;

public sealed partial class PromptValidationService(IPromptSafetyEvaluator? safetyEvaluator = null)
{
    private const string EmptyPromptRejectionMessage =
        "Could you please provide more details? Your message appears to be empty.";

    private const string SuspiciousPatternRejectionMessage =
        "I'm sorry, but I can't process that request. Please rephrase your question about books or our bookstore services.";

    [GeneratedRegex(
        @"(ignore\s+(previous|all|above)"
            + @"|system\s*:\s"
            + @"|role\s*:\s"
            + @"|<\|.*?\|>"
            + @"|###\s*instruction"
            + @"|\[INST\]"
            + "|<<SYS>>"
            + @"|<\|im_start\|>"
            + @"|\{\s*system_message"
            + @"|<script[\s>]"
            + @"|javascript:\s*"
            + @"|on\w+=\s*[""']"
            + @"|prompt\s*inject"
            + @"|do\s+not\s+follow"
            + @"|disregard\s+(all|your|the|previous)"
            + @"|you\s+are\s+now\s+a"
            + @"|act\s+as\s+(if|a|an)"
            + @"|reveal\s+(your|the|system)\s*(prompt|instruction)"
            + @"|<\/?\s*(div|span|iframe|img|a|body|html|head|form)\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    )]
    private static partial Regex SuspiciousPatternRegex();

    public PromptValidationOutcome Validate(string rawContent)
    {
        ArgumentNullException.ThrowIfNull(rawContent);

        var trimmed = rawContent.Trim();

        if (trimmed.Length < UserPrompt.MinLength)
        {
            return PromptValidationOutcome.Rejected(EmptyPromptRejectionMessage);
        }

        if (SuspiciousPatternRegex().IsMatch(trimmed))
        {
            return PromptValidationOutcome.Rejected(SuspiciousPatternRejectionMessage);
        }

        if (safetyEvaluator is not null)
        {
            var safetyResult = safetyEvaluator.Evaluate(trimmed);

            if (!safetyResult.IsSafe)
            {
                return PromptValidationOutcome.Rejected(
                    safetyResult.ViolationReason ?? SuspiciousPatternRejectionMessage
                );
            }
        }

        return PromptValidationOutcome.Accepted(UserPrompt.Create(trimmed));
    }
}
