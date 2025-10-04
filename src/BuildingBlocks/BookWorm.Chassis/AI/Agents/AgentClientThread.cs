namespace BookWorm.Chassis.AI.Agents;

public sealed class AgentClientThread(string? threadId = null)
{
    public string ThreadId { get; } = threadId ?? Guid.CreateVersion7().ToString("N");
}
