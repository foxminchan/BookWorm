using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(Constants.Other.Agents.RatingAgent)] AIAgent ratingAgent,
    [FromKeyedServices(Constants.Other.Agents.SummarizeAgent)] AIAgent summarizeAgent
) : ISummarizer
{
    public async Task<string?> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var workflow = AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(
                agents => new AgentWorkflowBuilder.RoundRobinGroupChatManager(agents)
                {
                    MaximumIterationCount = 2,
                }
            )
            .AddParticipants(ratingAgent, summarizeAgent)
            .Build();

        var workflowAgent = await workflow.AsAgentAsync();

        var response = await workflowAgent.RunAsync(
            $"Summarize the following content: {content}",
            cancellationToken: cancellationToken
        );

        return response.Text;
    }
}
