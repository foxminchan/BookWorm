using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(Constants.Other.Agents.RatingAgent)] AIAgent ratingAgent,
    [FromKeyedServices(Constants.Other.Agents.SummarizeAgent)] AIAgent summarizeAgent,
    [FromKeyedServices(Constants.Other.Agents.LanguageAgent)] AIAgent languageAgent,
    [FromKeyedServices(Constants.Other.Agents.RouterAgent)] AIAgent routerAgent,
    [FromKeyedServices(Constants.Other.Agents.SentimentAgent)] AIAgent sentimentAgent
) : ISummarizer
{
    public Workflow BuildAgentsWorkflow()
    {
        var workflow = AgentWorkflowBuilder
            .CreateHandoffBuilderWith(routerAgent)
            .WithHandoffs(
                routerAgent,
                [
                    languageAgent, // For non-English reviews
                    summarizeAgent, // For long verbose reviews
                    sentimentAgent, // For emotion-heavy reviews
                    ratingAgent, // Direct path for simple rating data
                ]
            )
            // Preprocessing agents all route to RatingAgent for final analysis
            .WithHandoff(languageAgent, ratingAgent) // After translation
            .WithHandoff(summarizeAgent, ratingAgent) // After condensing
            .WithHandoff(sentimentAgent, ratingAgent) // After sentiment analysis
            // RatingAgent can route back to RouterAgent if needed for multistep analysis
            .WithHandoff(ratingAgent, routerAgent)
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
            workflowAgentThread,
            cancellationToken: cancellationToken
        );

        return response.Text;
    }
}
