using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(Constants.Other.Agents.RatingAgent)] AIAgent ratingAgent,
    [FromKeyedServices(Constants.Other.Agents.SummarizeAgent)] AIAgent summarizeAgent,
    [FromKeyedServices(Constants.Other.Agents.LanguageAgent)] AIAgent languageAgent,
    [FromKeyedServices(Constants.Other.Agents.RouterAgent)] AIAgent routerAgent,
    [FromKeyedServices(Constants.Other.Agents.SentimentAgent)] AIAgent sentimentAgent,
    McpClient mcpClient
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
        var workflowAgent = BuildAgentsWorkflow().AsAgent();
        var workflowAgentThread = workflowAgent.GetNewThread();

        var prompt = await mcpClient.GetPromptAsync(
            "summarize_rating",
            new Dictionary<string, object?> { ["content"] = content },
            cancellationToken: cancellationToken
        );

        var response = await workflowAgent.RunAsync(
            prompt.ToChatMessages(),
            workflowAgentThread,
            cancellationToken: cancellationToken
        );

        return response.Text;
    }
}
