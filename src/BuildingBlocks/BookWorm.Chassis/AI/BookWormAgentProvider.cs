using SharpA2A.Core;

namespace BookWorm.Chassis.AI;

public sealed class BookWormAgentProvider : AgentProvider
{
    public static BookWormAgentProvider Create()
    {
        return new()
        {
            Organization = nameof(BookWorm),
        };
    }
}
