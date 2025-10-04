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

    public async Task<string> ProcessAgentsSequentiallyAsync(
        string message,
        CancellationToken cancellationToken = default
    )
    {
        var workflowAgent = await BuildAgentsWorkflow().AsAgentAsync();

        var response = await workflowAgent.RunAsync(message, cancellationToken: cancellationToken);

        return response.Text;
    }
}
