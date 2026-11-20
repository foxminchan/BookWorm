namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

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
}
