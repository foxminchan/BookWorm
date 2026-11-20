using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Questions;

public sealed class BinaryDecisionQuestion(
    string id,
    IReadOnlyList<AIContent> instructions,
    IReadOnlyList<AIContent>? trueDescription = null,
    IReadOnlyList<AIContent>? falseDescription = null
) : DecisionQuestion(id, instructions)
{
    public BinaryDecisionQuestion(
        string id,
        string instructions,
        string? trueDescription = null,
        string? falseDescription = null
    )
        : this(
            id,
            [new TextContent(instructions)],
            string.IsNullOrWhiteSpace(trueDescription) ? null : [new TextContent(trueDescription)],
            string.IsNullOrWhiteSpace(falseDescription) ? null : [new TextContent(falseDescription)]
        )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instructions);
    }

    public override DecisionKind Kind => DecisionKind.Binary;

    public IReadOnlyList<AIContent>? TrueDescription { get; } = trueDescription;

    public IReadOnlyList<AIContent>? FalseDescription { get; } = falseDescription;
}
