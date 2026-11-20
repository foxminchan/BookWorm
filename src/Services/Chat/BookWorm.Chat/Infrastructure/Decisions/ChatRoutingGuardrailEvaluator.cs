using BookWorm.Chassis.AI.Decisions.Abstractions;

namespace BookWorm.Chat.Infrastructure.Decisions;

internal static class ChatRoutingGuardrailEvaluator
{
    private const double ProbabilityThreshold = 0.5;

    internal static RoutingDecision? Evaluate(
        ChatMessage message,
        IReadOnlyList<ChatMessage> messages,
        double isSafeProbability,
        double isEnglishProbability,
        double isInScopeProbability,
        ILogger? logger
    )
    {
        if (isSafeProbability < ProbabilityThreshold)
        {
            logger?.LogInformation(
                "Decision router rejected unsafe message (P(is_safe)={Probability:F3})",
                isSafeProbability
            );

            return new(
                RouteTarget.UnsafeInput,
                message,
                ChatRoutingDecisionService.UnsafeInputMessage,
                1.0 - isSafeProbability,
                messages
            );
        }

        if (isEnglishProbability < ProbabilityThreshold)
        {
            logger?.LogInformation(
                "Decision router rejected non-English message (P(is_english)={Probability:F3})",
                isEnglishProbability
            );

            return new(
                RouteTarget.NonEnglish,
                message,
                ChatRoutingDecisionService.NonEnglishMessage,
                1.0 - isEnglishProbability,
                messages
            );
        }

        if (!(isInScopeProbability < ProbabilityThreshold))
        {
            return null;
        }

        logger?.LogInformation(
            "Decision router rejected out-of-scope message (P(is_in_scope)={Probability:F3})",
            isInScopeProbability
        );

        return new(
            RouteTarget.OutOfScope,
            message,
            ChatRoutingDecisionService.OutOfScopeMessage,
            1.0 - isInScopeProbability,
            messages
        );
    }

    internal static RoutingDecision HandlePartialResponse(
        ChatMessage message,
        IReadOnlyList<ChatMessage> messages,
        DecisionResponse response,
        ILogger? logger
    )
    {
        var isSafe = response.GetBinaryAnswer("is_safe")?.TrueProbability ?? 1.0;
        var isEnglish = response.GetBinaryAnswer("is_english")?.TrueProbability ?? 1.0;
        var isInScope = response.GetBinaryAnswer("is_in_scope")?.TrueProbability ?? 1.0;

        if (Evaluate(message, messages, isSafe, isEnglish, isInScope, logger) is { } rejected)
        {
            return rejected;
        }

        var routeChoice = response.GetChoiceAnswer("route");
        var target = routeChoice?.SelectedCandidateId switch
        {
            Constants.Other.Agents.QAAgent => RouteTarget.QA,
            Constants.Other.Agents.RatingAgent => RouteTarget.Rating,
            _ => RouteTarget.Book,
        };

        var confidence = routeChoice
            ?.Probabilities.FirstOrDefault(p =>
                string.Equals(p.Id, routeChoice.SelectedCandidateId, StringComparison.Ordinal)
            )
            ?.Value
            ?? 1.0;

        logger?.LogInformation(
            "Decision router selected {Target} with partial response (confidence={Confidence:F3})",
            target,
            confidence
        );

        return new(target, message, Confidence: confidence, Messages: messages);
    }
}
