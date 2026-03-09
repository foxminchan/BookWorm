using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace BookWorm.AI.Evaluation.Evaluators;

public sealed partial class ResponseLengthEvaluator(int maxWords = 300) : IEvaluator
{
    private const string ResponseLengthMetricName = "Response Length";

    public IReadOnlyCollection<string> EvaluationMetricNames => [ResponseLengthMetricName];

    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default
    )
    {
        var wordCount = string.IsNullOrWhiteSpace(modelResponse.Text)
            ? 0
            : WordPattern().Count(modelResponse.Text);

        var reason = $"The response contained {wordCount} words (max: {maxWords}).";

        var metric = new NumericMetric(ResponseLengthMetricName, wordCount, reason)
        {
            Interpretation =
                wordCount <= maxWords
                    ? new(
                        EvaluationRating.Good,
                        reason: $"The response was within the {maxWords}-word limit."
                    )
                    : new EvaluationMetricInterpretation(
                        EvaluationRating.Unacceptable,
                        true,
                        $"The response exceeded the {maxWords}-word limit with {wordCount} words."
                    ),
        };

        return new(new EvaluationResult(metric));
    }

    [GeneratedRegex(@"\b\w+\b")]
    private static partial Regex WordPattern();
}
