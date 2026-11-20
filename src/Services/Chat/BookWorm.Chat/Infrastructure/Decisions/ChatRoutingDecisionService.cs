using BookWorm.Chassis.AI.Decisions;

namespace BookWorm.Chat.Infrastructure.Decisions;

internal enum RouteTarget
{
    Book,
    QA,
    Rating,
    NonEnglish,
    OutOfScope,
}

internal sealed record RoutingDecision(
    RouteTarget Target,
    ChatMessage Message,
    string? RejectionReason = null,
    double Confidence = 1.0
);

/// <summary>
///     Evaluates incoming user messages against <see cref="IDecisionClient" /> (<c>Microsoft-Decision-1</c>)
///     to determine language compliance, domain scope, and target specialist agent.
/// </summary>
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

    internal static readonly IReadOnlyList<DecisionQuestion> RoutingQuestions =
    [
        new BinaryDecisionQuestion("is_english", "Is the user message written in English?"),
        new BinaryDecisionQuestion(
            "is_in_scope",
            "Is the user request related to BookWorm (books, reading, catalog, book reviews or ratings, bookstore policies, accounts, orders, or billing)?"
        ),
        new ChoiceDecisionQuestion(
            "route",
            "Which BookWorm specialist should handle this request?",
            [
                new(
                    Constants.Other.Agents.QAAgent,
                    "Questions about BookWorm store policies, services, accounts, orders, billing, or service complaints."
                ),
                new(
                    Constants.Other.Agents.RatingAgent,
                    "Requests to summarize reviews or analyze ratings ONLY when a specific book catalog ID (such as a UUID/GUID) is already present in the message."
                ),
                new(
                    Constants.Other.Agents.BookAgent,
                    "Book searches, recommendations, book information, conversational book feedback, or review/rating requests where the book is identified only by title, author, or description."
                ),
            ]
        ),
    ];

    public async ValueTask<RoutingDecision> EvaluateAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var response = await decisionClient
            .GetDecisionAsync(
                new DecisionRequest(message.Text, RoutingQuestions),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        var isEnglish = response.GetBinaryAnswer("is_english");
        if (isEnglish is not null && isEnglish.TrueProbability < ProbabilityThreshold)
        {
            logger?.LogInformation(
                "Decision router rejected non-English message (P(is_english)={Probability:F3})",
                isEnglish.TrueProbability
            );

            return new RoutingDecision(
                RouteTarget.NonEnglish,
                message,
                NonEnglishMessage,
                1.0 - isEnglish.TrueProbability
            );
        }

        var isInScope = response.GetBinaryAnswer("is_in_scope");
        if (isInScope is not null && isInScope.TrueProbability < ProbabilityThreshold)
        {
            logger?.LogInformation(
                "Decision router rejected out-of-scope message (P(is_in_scope)={Probability:F3})",
                isInScope.TrueProbability
            );

            return new RoutingDecision(
                RouteTarget.OutOfScope,
                message,
                OutOfScopeMessage,
                1.0 - isInScope.TrueProbability
            );
        }

        var routeChoice = response.GetChoiceAnswer("route");
        var target = routeChoice?.SelectedCandidateId switch
        {
            Constants.Other.Agents.QAAgent => RouteTarget.QA,
            Constants.Other.Agents.RatingAgent => RouteTarget.Rating,
            _ => RouteTarget.Book,
        };

        var confidence = routeChoice?.Confidence ?? 1.0;

        logger?.LogInformation(
            "Decision router selected {Target} (confidence={Confidence:F3})",
            target,
            confidence
        );

        return new RoutingDecision(target, message, Confidence: confidence);
    }
}
