using BookWorm.Chassis.AI.Decisions.Abstractions;
using BookWorm.Chat.Infrastructure.Decisions;
using BookWorm.Chat.Orchestration.Executors.Validation;

namespace BookWorm.Chat.Orchestration.Executors.Routing;

internal sealed class DecisionRouterExecutor(ChatRoutingDecisionService routingService)
    : Executor<AcceptedInput, RoutingDecision>(nameof(DecisionRouterExecutor))
{
    public DecisionRouterExecutor(
        IDecisionClient decisionClient,
        ILogger<ChatRoutingDecisionService>? logger = null
    )
        : this(new(decisionClient, logger)) { }

    public override ValueTask<RoutingDecision> HandleAsync(
        AcceptedInput message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        return routingService.EvaluateAsync(message.EffectiveMessages, cancellationToken);
    }

    public ValueTask<RoutingDecision> HandleAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default
    )
    {
        return routingService.EvaluateAsync(message, cancellationToken);
    }
}
