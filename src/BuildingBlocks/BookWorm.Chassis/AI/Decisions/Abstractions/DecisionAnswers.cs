using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions;

public sealed class DecisionProbability
{
    public DecisionProbability(string id, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        DecisionAnswer.ValidateProbability(value, nameof(value));

        Id = id;
        Value = value;
    }

    public string Id { get; }

    public double Value { get; }
}

public readonly record struct DecisionUnansweredReason
{
    public DecisionUnansweredReason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static DecisionUnansweredReason Refusal { get; } = new("refusal");

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

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

public sealed class BinaryDecisionAnswer : DecisionAnswer
{
    public BinaryDecisionAnswer(string questionId, double trueProbability)
        : base(questionId)
    {
        ValidateProbability(trueProbability, nameof(trueProbability));
        TrueProbability = trueProbability;
    }

    public override DecisionKind Kind => DecisionKind.Binary;

    public double TrueProbability { get; }
}

public sealed class ChoiceDecisionAnswer : DecisionAnswer
{
    public ChoiceDecisionAnswer(
        string questionId,
        string selectedCandidateId,
        IReadOnlyList<DecisionProbability> probabilities
    )
        : base(questionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedCandidateId);
        ValidateUniqueProbabilities(probabilities);

        SelectedCandidateId = selectedCandidateId;
        Probabilities = probabilities;
    }

    public override DecisionKind Kind => DecisionKind.Choice;

    public string SelectedCandidateId { get; }

    public IReadOnlyList<DecisionProbability> Probabilities { get; }
}

public sealed class ScoreDecisionAnswer : DecisionAnswer
{
    public ScoreDecisionAnswer(
        string questionId,
        double? score,
        IReadOnlyList<DecisionProbability> probabilities
    )
        : base(questionId)
    {
        if (score is { } s && (double.IsNaN(s) || double.IsInfinity(s) || s < 0.0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score,
                "Score must be a finite non-negative number."
            );
        }

        ValidateUniqueProbabilities(probabilities);

        Score = score;
        Probabilities = probabilities;
    }

    public override DecisionKind Kind => DecisionKind.Score;

    public double? Score { get; }

    public IReadOnlyList<DecisionProbability> Probabilities { get; }

    public double? GetExpectedScore(ScoreDecisionQuestion question)
    {
        ArgumentNullException.ThrowIfNull(question);

        var levelIndices = new Dictionary<string, int>(
            question.Levels.Count,
            StringComparer.Ordinal
        );
        for (var i = 0; i < question.Levels.Count; i++)
        {
            levelIndices[question.Levels[i].Id] = i;
        }

        var weightedSum = 0.0;
        var totalProbability = 0.0;

        foreach (var probability in Probabilities)
        {
            if (!levelIndices.TryGetValue(probability.Id, out var index))
            {
                continue;
            }

            weightedSum += index * probability.Value;
            totalProbability += probability.Value;
        }

        return totalProbability > 0.0 ? weightedSum / totalProbability : null;
    }
}

public sealed class UnansweredDecisionAnswer(
    string questionId,
    DecisionKind kind,
    DecisionUnansweredReason? reason = null
) : DecisionAnswer(questionId)
{
    public override DecisionKind Kind { get; } = kind;

    public DecisionUnansweredReason? Reason { get; } = reason;
}
