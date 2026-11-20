namespace BookWorm.Chassis.AI.Decisions.Abstractions.Answers;

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
