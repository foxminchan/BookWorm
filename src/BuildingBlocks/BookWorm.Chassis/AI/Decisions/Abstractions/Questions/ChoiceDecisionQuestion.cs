using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Questions;

public sealed class ChoiceDecisionQuestion : DecisionQuestion
{
    public ChoiceDecisionQuestion(
        string id,
        string instructions,
        IReadOnlyList<DecisionCandidate> candidates
    )
        : this(id, [new TextContent(instructions)], candidates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instructions);
    }

    private ChoiceDecisionQuestion(
        string id,
        IReadOnlyList<AIContent> instructions,
        IReadOnlyList<DecisionCandidate> candidates
    )
        : base(id, instructions)
    {
        ValidateUniqueIds(
            candidates,
            static c => c.Id,
            "At least one candidate is required.",
            "candidate",
            nameof(candidates)
        );

        Candidates = candidates;
    }

    public override DecisionKind Kind => DecisionKind.Choice;

    public IReadOnlyList<DecisionCandidate> Candidates { get; }
}
