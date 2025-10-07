using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class AgentOrchestrationService(OrchestrateAgents orchestrateAgents)
    : IAgentOrchestrationService
{
    public Workflow BuildAgentsWorkflow()
    {
        var workflow = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(orchestrateAgents.RouterAgent)
            .WithHandoffs(
                orchestrateAgents.RouterAgent,
                [
                    orchestrateAgents.LanguageAgent, // For non-English input
                    orchestrateAgents.SummarizeAgent, // For long/complex messages
                    orchestrateAgents.SentimentAgent, // For feedback/emotions
                    orchestrateAgents.BookAgent, // For direct book queries (default path)
                ]
            )
            // LanguageAgent translates non-English input and routes to BookAgent
            .WithHandoff(orchestrateAgents.LanguageAgent, orchestrateAgents.BookAgent)
            // SummarizeAgent condenses verbose messages and routes to BookAgent
            .WithHandoff(orchestrateAgents.SummarizeAgent, orchestrateAgents.BookAgent)
            // SentimentAgent analyzes sentiment and can route to BookAgent or back to Router
            .WithHandoffs(
                orchestrateAgents.SentimentAgent,
                [orchestrateAgents.BookAgent, orchestrateAgents.RouterAgent]
            )
            // BookAgent handles book queries and can hand back to Router for follow-ups
            .WithHandoff(orchestrateAgents.BookAgent, orchestrateAgents.RouterAgent)
            .Build();

        return workflow;
    }

    public async Task<IAsyncEnumerable<AgentRunResponseUpdate>> RunWorkflowStreamingAsync(
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
