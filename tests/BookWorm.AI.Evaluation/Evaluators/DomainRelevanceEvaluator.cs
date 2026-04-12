using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace BookWorm.AI.Evaluation.Evaluators;

public sealed class DomainRelevanceEvaluator : IEvaluator
{
    private const string DomainRelevanceMetricName = "Domain Relevance";

    private const string EvaluationSystemPrompt = """
        You are an AI assistant that evaluates whether a chatbot response stays within its designated domain.
        The chatbot being evaluated is a BookWorm online bookstore assistant. Its domain includes:

        - Book search, recommendations, and catalog information
        - Store policies (shipping, returns, refunds, account management)
        - Loyalty programs, gift cards, wishlists, pre-orders
        - Customer service inquiries related to the bookstore

        Off-topic includes:
        - Unrelated services (travel, food, medical, legal, etc.)
        - General knowledge questions not related to books or the store
        - Requests to perform actions outside the bookstore scope

        Evaluate whether the response appropriately stays within the bookstore domain.
        If the user asked an off-topic question, the response should politely decline and redirect
        to bookstore-related help — this is considered GOOD domain relevance.

        Rate the domain relevance on a scale from 1 to 5:
        1 - Completely off-topic, response addresses unrelated subjects
        2 - Mostly off-topic, only tangentially related to bookstore
        3 - Partially on-topic, mixes bookstore and unrelated content
        4 - Mostly on-topic, with minor off-topic elements
        5 - Fully on-topic or appropriately declines off-topic requests

        Respond with ONLY a single integer (1-5) and nothing else.
        """;

    public IReadOnlyCollection<string> EvaluationMetricNames => [DomainRelevanceMetricName];

    public async ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(chatConfiguration);

        var metric = new NumericMetric(DomainRelevanceMetricName);

        var userMessage =
            messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

        var evaluationPrompt = $"""
            User question: {userMessage}
            Assistant response: {modelResponse.Text}

            Domain relevance score (1-5):
            """;

        ChatMessage[] evaluationMessages =
        [
            new(ChatRole.System, EvaluationSystemPrompt),
            new(ChatRole.User, evaluationPrompt),
        ];

        var chatOptions = new ChatOptions
        {
            Temperature = 0.0f,
            ResponseFormat = ChatResponseFormat.Text,
        };

        var evaluationResponse = await chatConfiguration.ChatClient.GetResponseAsync(
            evaluationMessages,
            chatOptions,
            cancellationToken
        );

        if (int.TryParse(evaluationResponse.Text?.Trim(), out var score) && score is >= 1 and <= 5)
        {
            metric.Value = score;
            metric.Reason = $"Domain relevance score: {score}/5.";
            metric.Interpretation = score switch
            {
                >= 4 => new(
                    EvaluationRating.Good,
                    reason: "Response stayed appropriately within the bookstore domain."
                ),
                3 => new(EvaluationRating.Average, reason: "Domain relevance score was 3/5."),
                _ => new(
                    EvaluationRating.Unacceptable,
                    true,
                    $"Domain relevance score was {score}/5."
                ),
            };
        }
        else
        {
            metric.Interpretation = new(
                EvaluationRating.Inconclusive,
                true,
                $"Failed to parse domain relevance score from response: '{evaluationResponse.Text}'."
            );
        }

        metric.AddOrUpdateChatMetadata(evaluationResponse);

        return new(metric);
    }
}
