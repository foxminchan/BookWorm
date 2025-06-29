using BookWorm.Chat.Agents;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatContext(
    IMcpClient mcpClient,
    [FromKeyedServices(nameof(BookAgent))] Agent bookAgent,
    [FromKeyedServices(nameof(LanguageAgent))] Agent languageAgent,
    [FromKeyedServices(nameof(SentimentAgent))] Agent sentimentAgent,
    [FromKeyedServices(nameof(SummarizeAgent))] Agent summarizeAgent
)
{
    public IMcpClient McpClient { get; } = mcpClient;
    public Agent BookAgent { get; } = bookAgent;
    public Agent LanguageAgent { get; } = languageAgent;
    public Agent SentimentAgent { get; } = sentimentAgent;
    public Agent SummarizeAgent { get; } = summarizeAgent;
}
