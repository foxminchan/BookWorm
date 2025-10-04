using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public interface ISummarizer
{
    Workflow BuildAgentsWorkflow();
    Task<string?> SummarizeAsync(string content, CancellationToken cancellationToken = default);
}
