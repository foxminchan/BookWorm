namespace BookWorm.Chat.Orchestration.Conditions;

internal static class InputValidationCondition
{
    public static bool IsAccepted(ChatMessage? message) => message?.Role == ChatRole.User;

    public static bool IsRejected(ChatMessage? message) => message?.Role == ChatRole.Assistant;
}
