namespace BookWorm.AI.Evaluation.Evaluators;

/// <summary>
/// An <see cref="IEvaluator"/> that evaluates whether a response contains relevant book information.
/// </summary>
public sealed class BookRelevanceEvaluator : IEvaluator
{
    public const string BookRelevanceMetricName = "BookRelevance";

    /// <inheritdoc/>
    public IReadOnlyCollection<string> EvaluationMetricNames => [BookRelevanceMetricName];

    /// <summary>
    /// Checks if the response contains book-related information.
    /// </summary>
    private static bool ContainsBookInformation(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return false;
        }

        // Check for common book-related keywords
        string[] bookKeywords =
        [
            "book", "author", "title", "isbn", "publisher", "genre",
            "fiction", "non-fiction", "novel", "chapter", "page",
            "reading", "literature", "story", "publication"
        ];

        return bookKeywords.Any(keyword =>
            response.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Provides a default interpretation for the supplied <paramref name="metric"/>.
    /// </summary>
    private static void Interpret(BooleanMetric metric)
    {
        if (metric.Value is null)
        {
            metric.Interpretation = new EvaluationMetricInterpretation(
                EvaluationRating.Unknown,
                failed: true,
                reason: "Failed to evaluate book relevance for the response.");
        }
        else if (metric.Value == true)
        {
            metric.Interpretation = new EvaluationMetricInterpretation(
                EvaluationRating.Good,
                reason: "The response contains relevant book information.");
        }
        else
        {
            metric.Interpretation = new EvaluationMetricInterpretation(
                EvaluationRating.Unacceptable,
                failed: true,
                reason: "The response does not contain relevant book information.");
        }
    }

    /// <inheritdoc/>
    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        // Check if the response contains book-related information
        bool hasBookInfo = ContainsBookInformation(modelResponse.Text);

        string reason = hasBookInfo
            ? $"The response contains book-related information."
            : $"The response does not contain book-related information.";

        // Create a BooleanMetric with the evaluation result
        var metric = new BooleanMetric(BookRelevanceMetricName, value: hasBookInfo, reason);

        // Attach a default interpretation for the metric
        Interpret(metric);

        return new ValueTask<EvaluationResult>(new EvaluationResult(metric));
    }
}
