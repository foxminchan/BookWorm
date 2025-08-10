using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class OrchestrateAgents(
    [FromKeyedServices(nameof(BookAgent))] ChatCompletionAgent bookAgent,
    [FromKeyedServices(nameof(LanguageAgent))] ChatCompletionAgent languageAgent,
    [FromKeyedServices(nameof(SentimentAgent))] ChatCompletionAgent sentimentAgent,
    [FromKeyedServices(nameof(SummarizeAgent))] ChatCompletionAgent summarizeAgent
)
{
    public ChatCompletionAgent BookAgent { get; } = bookAgent;
    public ChatCompletionAgent LanguageAgent { get; } = languageAgent;
    public ChatCompletionAgent SentimentAgent { get; } = sentimentAgent;
    public ChatCompletionAgent SummarizeAgent { get; } = summarizeAgent;
}
