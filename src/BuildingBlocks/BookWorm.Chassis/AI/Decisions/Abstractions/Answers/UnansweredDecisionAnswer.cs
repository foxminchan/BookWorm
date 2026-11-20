namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

public sealed class UnansweredDecisionAnswer(
    string questionId,
    DecisionKind kind,
    DecisionUnansweredReason? reason = null
) : DecisionAnswer(questionId)
{
    public override DecisionKind Kind { get; } = kind;

    public DecisionUnansweredReason? Reason { get; } = reason;
}
