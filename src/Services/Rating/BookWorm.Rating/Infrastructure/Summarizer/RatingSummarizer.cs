using BookWorm.Constants.Other;
using Microsoft.Agents.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(Workflows.RatingSummarizer)] AIAgent ratingSummarizerAgent,
    McpClient mcpClient
) : ISummarizer
{
    public async Task<string?> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var workflowAgentThread = await ratingSummarizerAgent.GetNewSessionAsync(cancellationToken);

        var prompt = await mcpClient.GetPromptAsync(
            "summarize_rating",
            new Dictionary<string, object?> { ["content"] = content },
            cancellationToken: cancellationToken
        );

        var response = await ratingSummarizerAgent.RunAsync(
            prompt.ToChatMessages(),
            workflowAgentThread,
            cancellationToken: cancellationToken
        );

        return response.Text;
    }
}
