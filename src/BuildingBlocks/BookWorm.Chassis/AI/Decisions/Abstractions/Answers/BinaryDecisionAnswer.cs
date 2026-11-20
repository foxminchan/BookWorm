namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

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
