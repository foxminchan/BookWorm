using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(Constants.Other.Agents.RatingAgent)] ChatCompletionAgent agent
) : ISummarizer
{
    public async Task<string?> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default
    )
    {
        ChatHistoryAgentThread agentThread = new();

        var agentResponse = agent.InvokeAsync(
            $"Summarize the following content: {content}",
            agentThread,
            cancellationToken: cancellationToken
        );

        await foreach (var item in agentResponse.ConfigureAwait(false))
        {
            if (!string.IsNullOrWhiteSpace(item.Message.Content))
            {
                return item.Message.Content;
            }
        }

        return null;
    }
}
