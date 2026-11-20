using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

public abstract class DecisionAnswer
{
    private protected DecisionAnswer(string questionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        QuestionId = questionId;
    }

    public string QuestionId { get; }

    public abstract DecisionKind Kind { get; }

    public object? RawRepresentation { get; set; }

    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    internal static void ValidateProbability(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value is < 0.0 or > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                "Probability must be a finite number between 0 and 1."
            );
        }
    }

    internal static void ValidateUniqueProbabilities(
        IReadOnlyList<DecisionProbability> probabilities
    )
    {
        ArgumentNullException.ThrowIfNull(probabilities);
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var probability in probabilities)
        {
            ArgumentNullException.ThrowIfNull(probability);
            if (!seenIds.Add(probability.Id))
            {
                throw new ArgumentException(
                    $"Duplicate probability ID '{probability.Id}'.",
                    nameof(probabilities)
                );
            }
        }
    }
}
