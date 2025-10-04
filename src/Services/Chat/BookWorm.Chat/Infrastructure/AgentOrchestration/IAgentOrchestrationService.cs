using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public interface IAgentOrchestrationService
{
    Workflow BuildAgentsWorkflow();

    Task<IAsyncEnumerable<AgentRunResponseUpdate>> ProcessAgentsSequentiallyAsync(
        string message,
        CancellationToken cancellationToken = default
    );
}
