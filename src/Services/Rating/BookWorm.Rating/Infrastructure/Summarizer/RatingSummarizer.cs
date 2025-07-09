using BookWorm.Rating.Agents;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public sealed class RatingSummarizer(
    [FromKeyedServices(nameof(RatingAgent))] ChatCompletionAgent agent
) : ISummarizer
{
    public async Task<string?> SummarizeAsync(
        string content,
        CancellationToken cancellationToken = default
    )
    {
        ChatHistoryAgentThread agentThread = new();

        var result = await agent
            .InvokeAsync(
                $"Summarize the following content: {content}",
                agentThread,
                cancellationToken: cancellationToken
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Message.Content))
        {
            return null;
        }

        return result.Message.Content;
    }
}
