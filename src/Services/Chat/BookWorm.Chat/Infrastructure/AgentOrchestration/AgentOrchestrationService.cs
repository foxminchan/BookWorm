using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class AgentOrchestrationService(OrchestrateAgents orchestrateAgents)
    : IAgentOrchestrationService
{
    public Workflow BuildAgentsWorkflow()
    {
        var workflow = AgentWorkflowBuilder.BuildSequential(
            orchestrateAgents.LanguageAgent,
            orchestrateAgents.SummarizeAgent,
            orchestrateAgents.SentimentAgent,
            orchestrateAgents.BookAgent
        );

        return workflow;
    }

    public async Task<IAsyncEnumerable<AgentRunResponseUpdate>> ProcessAgentsSequentiallyAsync(
        string message,
        CancellationToken cancellationToken = default
    )
    {
        var workflowAgent = await BuildAgentsWorkflow().AsAgentAsync();
        var workflowAgentThread = workflowAgent.GetNewThread();

        var response = workflowAgent.RunStreamingAsync(
            message,
            workflowAgentThread,
            cancellationToken: cancellationToken
        );

        return response;
    }
}
