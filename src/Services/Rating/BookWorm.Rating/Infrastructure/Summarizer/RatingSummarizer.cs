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
            .WithHandoffs(routerAgent, [languageAgent, summarizeAgent, sentimentAgent, ratingAgent])
            .WithHandoff(
                languageAgent,
                ratingAgent,
                "Transfer to this agent if the user input is not in English."
            )
            .WithHandoff(
                summarizeAgent,
                ratingAgent,
                "Transfer to this agent if the user message is very long or complex."
            )
            .WithHandoff(
                sentimentAgent,
                ratingAgent,
                "Transfer to this agent if the user message contains emotional content."
            )
            .WithHandoff(
                ratingAgent,
                routerAgent,
                "Transfer back to RouterAgent for any follow-up handling."
            )
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
