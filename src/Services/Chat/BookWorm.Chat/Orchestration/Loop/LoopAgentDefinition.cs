namespace BookWorm.Chat.Orchestration.Loop;

internal static class LoopAgentDefinition
{
    public const string Name = "chat-loop";

    public const string Description =
        "A bounded Chat workflow that iteratively closes gaps in its answer before returning.";
}
