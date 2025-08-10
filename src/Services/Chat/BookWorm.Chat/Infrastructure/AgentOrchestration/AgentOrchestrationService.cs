using BookWorm.Chat.Features;
using BookWorm.Chat.Infrastructure.Backplane;
using BookWorm.Chat.Infrastructure.ChatHistory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration;

public sealed class AgentOrchestrationService(
    OrchestrateAgents orchestrateAgents,
    IChatHistoryService chatHistoryService,
    RedisBackplaneService backplaneService,
    ILogger<AgentOrchestrationService> logger
) : IAgentOrchestrationService
{
    public async Task<string> ProcessAgentsSequentiallyAsync(
        string userMessage,
        Guid conversationId,
        Guid assistantReplyId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Starting agent orchestration for conversation {ConversationId}",
            conversationId
        );

        // Use concurrent orchestration to process responses from both agents in parallel
        var runtime = new InProcessRuntime();
        await runtime.StartAsync(cancellationToken);

        SequentialOrchestration orchestration = new(
            orchestrateAgents.LanguageAgent,
            orchestrateAgents.SummarizeAgent,
            orchestrateAgents.SentimentAgent,
            orchestrateAgents.BookAgent
        )
        {
            ResponseCallback = ResponseCallbackAsync,
        };

        // Get result from sequential orchestration - it will handle streaming internally
        var result = await orchestration.InvokeAsync(userMessage, runtime, cancellationToken);

        // Get final combined results
        var finalResults = await result.GetValueAsync(TimeSpan.FromSeconds(60), cancellationToken);

        var combinedMessage = string.Join("\n\n", finalResults);

        await runtime.RunUntilIdleAsync();

        logger.LogInformation(
            "Agent orchestration completed for conversation {ConversationId}",
            conversationId
        );

        return combinedMessage;

        // Callback to observe agent responses during orchestration
        async ValueTask ResponseCallbackAsync(ChatMessageContent response)
        {
            // Add to existing message history for tracking
            var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory { response };
            chatHistoryService.AddMessages(history);

            logger.LogInformation(
                "Agent response received from {AuthorName}: {Content}",
                response.AuthorName ?? "Anonymous User",
                response.Content
            );

            // Stream individual agent responses as they come in
            var agentFragment = new ClientMessageFragment(
                assistantReplyId,
                AuthorRole.Assistant.Label,
                $"**{response.AuthorName ?? "Agent"}**: {response.Content}\n\n",
                Guid.CreateVersion7()
            );

            await backplaneService.ConversationState.PublishFragmentAsync(
                conversationId,
                agentFragment
            );
        }
    }
}
