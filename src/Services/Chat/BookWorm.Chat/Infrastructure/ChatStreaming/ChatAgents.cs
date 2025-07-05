using BookWorm.Chat.Agents;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatAgents(
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
