using BookWorm.Rating.Infrastructure.Agents;
using Microsoft.Agents.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.Summarizer;

internal sealed class RatingSummarizer(
    [FromKeyedServices(RatingAgent.Name)] AIAgent ratingAgent,
    McpClient mcpClient
) : ISummarizer
{
    public async Task<string?> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var prompt = await mcpClient.GetPromptAsync(
            "summarize_rating",
            new Dictionary<string, object?> { ["content"] = content },
            cancellationToken: cancellationToken
        );

        var agentSession = await ratingAgent.CreateSessionAsync(cancellationToken);
        var response = await ratingAgent.RunAsync(
            prompt.ToChatMessages(),
            agentSession,
            cancellationToken: cancellationToken
        );

        return response.Text;
    }
}
