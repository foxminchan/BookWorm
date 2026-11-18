using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

namespace BookWorm.Chat.Domain.Services;

public sealed record PromptValidationOutcome(
    bool IsAccepted,
    UserPrompt? Prompt,
    string? RejectionReason
)
{
    public static PromptValidationOutcome Accepted(UserPrompt prompt)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        return new(true, prompt, null);
    }

    public static PromptValidationOutcome Rejected(string reason)
    {
        return new(false, null, reason);
    }
}
