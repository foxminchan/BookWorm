using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(Constants.Other.Agents.RatingAgent)] AIAgent ratingAgent,
    [FromKeyedServices(Constants.Other.Agents.SummarizeAgent)] AIAgent summarizeAgent,
    [FromKeyedServices(Constants.Other.Agents.LanguageAgent)] AIAgent languageAgent
) : ISummarizer
{
    public Workflow BuildAgentsWorkflow()
    {
        var workflow = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(languageAgent)
            .WithHandoff(languageAgent, summarizeAgent)
            .WithHandoff(summarizeAgent, ratingAgent)
            .WithHandoff(ratingAgent, languageAgent)
            .Build();

        return workflow;
    }

    public async Task<string?> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var workflowAgent = await BuildAgentsWorkflow().AsAgentAsync();
        var workflowAgentThread = workflowAgent.GetNewThread();

        var response = await workflowAgent.RunAsync(
            $"Summarize the following content: {content}",
            thread: workflowAgentThread,
            cancellationToken: cancellationToken
        );

        return response.Text;
    }
}
