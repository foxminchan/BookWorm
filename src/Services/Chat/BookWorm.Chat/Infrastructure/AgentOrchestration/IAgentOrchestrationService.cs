namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public interface IAgentOrchestrationService
{
    Task<string> ProcessAgentsSequentiallyAsync(
        string userMessage,
        Guid conversationId,
        Guid assistantReplyId,
        CancellationToken cancellationToken = default
    );
}
