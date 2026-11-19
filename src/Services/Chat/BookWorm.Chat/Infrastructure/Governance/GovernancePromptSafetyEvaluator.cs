using AgentGovernance;
using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;
using BookWorm.Chat.Domain.Services;

namespace BookWorm.Chat.Infrastructure.Governance;

internal sealed class GovernancePromptSafetyEvaluator(GovernanceKernel? governanceKernel = null)
    : IPromptSafetyEvaluator
{
    public PromptSafetyResult Evaluate(string content)
    {
        if (governanceKernel?.InjectionDetector is not { } detector)
        {
            return PromptSafetyResult.Safe();
        }

        var injectionResult = detector.Detect(content);

        return injectionResult.IsInjection
            ? PromptSafetyResult.Unsafe(
                $"""
                I'm sorry, but I can't process that request due to a security policy ({injectionResult.InjectionType}). 
                Please rephrase your question about books or our bookstore services.
                """,
                injectionResult.InjectionType.ToString()
            )
            : PromptSafetyResult.Safe();
    }
}
