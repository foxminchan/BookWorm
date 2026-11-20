using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Questions;

public sealed class ScoreDecisionQuestion : DecisionQuestion
{
    public ScoreDecisionQuestion(
        string id,
        string instructions,
        IReadOnlyList<DecisionScoreLevel> levels
    )
        : this(id, [new TextContent(instructions)], levels)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instructions);
    }

    private ScoreDecisionQuestion(
        string id,
        IReadOnlyList<AIContent> instructions,
        IReadOnlyList<DecisionScoreLevel> levels
    )
        : base(id, instructions)
    {
        ValidateUniqueIds(
            levels,
            static l => l.Id,
            "At least one score level is required.",
            "score level",
            nameof(levels)
        );

        Levels = levels;
    }

    public override DecisionKind Kind => DecisionKind.Score;

    public IReadOnlyList<DecisionScoreLevel> Levels { get; }
}
