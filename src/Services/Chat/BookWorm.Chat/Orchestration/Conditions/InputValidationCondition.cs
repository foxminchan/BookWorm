namespace BookWorm.Chat.Orchestration.Conditions;

internal static class InputValidationCondition
{
    public static bool IsAccepted(ChatMessage? message)
    {
        return message?.Role == ChatRole.User;
    }

    public static bool IsRejected(ChatMessage? message)
    {
        return message?.Role == ChatRole.Assistant;
    }
}
