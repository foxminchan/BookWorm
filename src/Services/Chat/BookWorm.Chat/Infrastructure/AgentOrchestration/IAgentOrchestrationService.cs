using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public interface IAgentOrchestrationService
{
    Workflow BuildAgentsWorkflow();

    Task<string> ProcessAgentsSequentiallyAsync(
        string message,
        CancellationToken cancellationToken = default
    );
}
