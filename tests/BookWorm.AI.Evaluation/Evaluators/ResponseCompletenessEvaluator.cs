namespace BookWorm.AI.Evaluation.Evaluators;

/// <summary>
/// An <see cref="IEvaluator"/> that evaluates the response length and completeness.
/// </summary>
public sealed class ResponseCompletenessEvaluator : IEvaluator
{
    public const string CompletenessMetricName = "ResponseCompleteness";

    private readonly int _minimumCharacters;
    private readonly int _maximumCharacters;

    /// <inheritdoc/>
    public IReadOnlyCollection<string> EvaluationMetricNames => [CompletenessMetricName];

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseCompletenessEvaluator"/> class.
    /// </summary>
    /// <param name="minimumCharacters">The minimum expected character count for a complete response.</param>
    /// <param name="maximumCharacters">The maximum expected character count for a complete response.</param>
    public ResponseCompletenessEvaluator(int minimumCharacters = 50, int maximumCharacters = 2000)
    {
        _minimumCharacters = minimumCharacters;
        _maximumCharacters = maximumCharacters;
    }

    /// <summary>
    /// Calculates the completeness score based on response length.
    /// </summary>
    private double CalculateCompletenessScore(string? response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return 0.0;
        }

        int length = response.Length;

        if (length < _minimumCharacters)
        {
            // Too short: score between 0 and 3
            return Math.Min(3.0, (double)length / _minimumCharacters * 3.0);
        }

        if (length > _maximumCharacters)
        {
            // Too long: score between 3 and 4
            double excessRatio = (double)(length - _maximumCharacters) / _maximumCharacters;
            return Math.Max(3.0, 5.0 - excessRatio);
        }

        // Optimal length: score 5
        return 5.0;
    }

    /// <summary>
    /// Provides a default interpretation for the supplied <paramref name="metric"/>.
    /// </summary>
    private void Interpret(NumericMetric metric)
    {
        if (metric.Value is null)
        {
            metric.Interpretation = new EvaluationMetricInterpretation(
                EvaluationRating.Unknown,
                failed: true,
                reason: "Failed to calculate completeness score for the response.");
            return;
        }

        metric.Interpretation = metric.Value switch
        {
            >= 4.5 => new EvaluationMetricInterpretation(
                EvaluationRating.Exceptional,
                reason: $"The response length ({metric.Value:F1}) is optimal and complete."),
            >= 3.5 => new EvaluationMetricInterpretation(
                EvaluationRating.Good,
                reason: $"The response length ({metric.Value:F1}) is acceptable."),
            >= 2.0 => new EvaluationMetricInterpretation(
                EvaluationRating.Average,
                reason: $"The response length ({metric.Value:F1}) is somewhat limited."),
            _ => new EvaluationMetricInterpretation(
                EvaluationRating.Unacceptable,
                failed: true,
                reason: $"The response length ({metric.Value:F1}) is insufficient or excessive.")
        };
    }

    /// <inheritdoc/>
    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        // Calculate the completeness score
        double score = CalculateCompletenessScore(modelResponse.Text);
        int length = modelResponse.Text?.Length ?? 0;

        string reason = $"The response contains {length} characters. " +
                       $"Expected range: {_minimumCharacters}-{_maximumCharacters} characters.";

        // Create a NumericMetric with the score
        var metric = new NumericMetric(CompletenessMetricName, value: score, reason);

        // Attach a default interpretation for the metric
        Interpret(metric);

        return new ValueTask<EvaluationResult>(new EvaluationResult(metric));
    }
}
