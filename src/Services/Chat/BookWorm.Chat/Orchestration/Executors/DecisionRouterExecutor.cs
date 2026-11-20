using BookWorm.Chassis.AI.Decisions;
using BookWorm.Chat.Infrastructure.Decisions;

namespace BookWorm.Chat.Orchestration.Executors;

/// <summary>
///     Routes accepted user messages using <see cref="ChatRoutingDecisionService" />
///     (<c>Microsoft-Decision-1</c>) instead of a generative LLM router agent.
/// </summary>
internal sealed class DecisionRouterExecutor(ChatRoutingDecisionService routingService)
    : Executor<ChatMessage, RoutingDecision>(nameof(DecisionRouterExecutor))
{
    public DecisionRouterExecutor(
        IDecisionClient decisionClient,
        ILogger<ChatRoutingDecisionService>? logger = null
    )
        : this(new ChatRoutingDecisionService(decisionClient, logger)) { }

    public override ValueTask<RoutingDecision> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        return routingService.EvaluateAsync(message, cancellationToken);
    }
}
