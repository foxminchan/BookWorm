using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class OrchestrateAgents(
    [FromKeyedServices(Constants.Other.Agents.BookAgent)] ChatCompletionAgent bookAgent,
    [FromKeyedServices(Constants.Other.Agents.LanguageAgent)] ChatCompletionAgent languageAgent,
    [FromKeyedServices(Constants.Other.Agents.SentimentAgent)] ChatCompletionAgent sentimentAgent,
    [FromKeyedServices(Constants.Other.Agents.SummarizeAgent)] ChatCompletionAgent summarizeAgent
)
{
    public ChatCompletionAgent BookAgent { get; } = bookAgent;
    public ChatCompletionAgent LanguageAgent { get; } = languageAgent;
    public ChatCompletionAgent SentimentAgent { get; } = sentimentAgent;
    public ChatCompletionAgent SummarizeAgent { get; } = summarizeAgent;
}
