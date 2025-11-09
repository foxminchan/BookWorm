using Microsoft.Agents.AI;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class OrchestrateAgents(
    [FromKeyedServices(Constants.Other.Agents.RouterAgent)] AIAgent routerAgent,
    [FromKeyedServices(Constants.Other.Agents.BookAgent)] AIAgent bookAgent,
    [FromKeyedServices(Constants.Other.Agents.LanguageAgent)] AIAgent languageAgent,
    [FromKeyedServices(Constants.Other.Agents.SentimentAgent)] AIAgent sentimentAgent,
    [FromKeyedServices(Constants.Other.Agents.SummarizeAgent)] AIAgent summarizeAgent,
    [FromKeyedServices(Constants.Other.Agents.QAAgent)] AIAgent qaAgent
)
{
    public AIAgent RouterAgent { get; } = routerAgent;
    public AIAgent BookAgent { get; } = bookAgent;
    public AIAgent LanguageAgent { get; } = languageAgent;
    public AIAgent SentimentAgent { get; } = sentimentAgent;
    public AIAgent SummarizeAgent { get; } = summarizeAgent;
    public AIAgent QAAgent { get; } = qaAgent;
}
