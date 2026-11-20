using BookWorm.Chassis.AI.Decisions;

namespace BookWorm.Chat.Infrastructure.Decisions;

internal sealed class ChatRoutingDecisionService(
    IDecisionClient decisionClient,
    ILogger<ChatRoutingDecisionService>? logger = null
)
{
    internal const string NonEnglishMessage =
        "BookWorm Chat currently supports English only. Please resend your request in English.";

    internal const string OutOfScopeMessage =
        "I can only help with BookWorm bookstore topics such as book search, recommendations, reviews, orders, or store policies.";

    internal const double ProbabilityThreshold = 0.5;

    internal static readonly DecisionDefinition<ChatRoutingResult> RoutingDefinition =
        DecisionDefinition<ChatRoutingResult>.Create(builder =>
        {
            builder.BinaryProbability(r => r.IsEnglish, "Is the user message written in English?");
            builder.BinaryProbability(
                r => r.IsInScope,
                "Is the user request related to BookWorm (books, reading, catalog, book reviews or ratings, bookstore policies, accounts, orders, or billing)?"
            );
            builder.Choice(r => r.Route, "Which BookWorm specialist should handle this request?");
        });

    internal static IReadOnlyList<DecisionQuestion> RoutingQuestions => RoutingDefinition.Questions;

    internal async ValueTask<RoutingDecision> EvaluateAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        try
        {
            var typedResponse = await decisionClient
                .GetDecisionAsync(
                    message.Text,
                    RoutingDefinition,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);

            var result = typedResponse.Result;

            if (EvaluateGuardrails(message, result.IsEnglish, result.IsInScope) is { } rejected)
            {
                return rejected;
            }

            var target = MapRouteTarget(result.Route);
            var distribution = typedResponse.GetDistribution(r => r.Route);
            var confidence = distribution.TryGetValue(result.Route, out var p) ? p : 1.0;

            logger?.LogInformation(
                "Decision router selected {Target} (confidence={Confidence:F3})",
                target,
                confidence
            );

            return new RoutingDecision(target, message, Confidence: confidence);
        }
        catch (DecisionUnansweredException ex)
        {
            return HandlePartialResponse(message, ex.Response);
        }
    }

    private RoutingDecision? EvaluateGuardrails(
        ChatMessage message,
        double isEnglishProbability,
        double isInScopeProbability
    )
    {
        if (isEnglishProbability < ProbabilityThreshold)
        {
            logger?.LogInformation(
                "Decision router rejected non-English message (P(is_english)={Probability:F3})",
                isEnglishProbability
            );

            return new RoutingDecision(
                RouteTarget.NonEnglish,
                message,
                NonEnglishMessage,
                1.0 - isEnglishProbability
            );
        }

        if (isInScopeProbability < ProbabilityThreshold)
        {
            logger?.LogInformation(
                "Decision router rejected out-of-scope message (P(is_in_scope)={Probability:F3})",
                isInScopeProbability
            );

            return new RoutingDecision(
                RouteTarget.OutOfScope,
                message,
                OutOfScopeMessage,
                1.0 - isInScopeProbability
            );
        }

        return null;
    }

    private RoutingDecision HandlePartialResponse(ChatMessage message, DecisionResponse response)
    {
        var isEnglish = response.GetBinaryAnswer("is_english")?.TrueProbability ?? 1.0;
        var isInScope = response.GetBinaryAnswer("is_in_scope")?.TrueProbability ?? 1.0;

        if (EvaluateGuardrails(message, isEnglish, isInScope) is { } rejected)
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

        return new RoutingDecision(target, message, Confidence: confidence);
    }

    private static RouteTarget MapRouteTarget(ChatRouteCandidate candidate)
    {
        return candidate switch
        {
            ChatRouteCandidate.QA => RouteTarget.QA,
            ChatRouteCandidate.Rating => RouteTarget.Rating,
            _ => RouteTarget.Book,
        };
    }
}
