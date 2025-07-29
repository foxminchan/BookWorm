using SharpA2A.Core;

namespace BookWorm.Chassis.RAG.Agent;

public sealed class BookWormAgentProvider : AgentProvider
{
    public static BookWormAgentProvider Create()
    {
        return new() { Organization = nameof(BookWorm) };
    }
}
