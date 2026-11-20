using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions;

public abstract class DecisionQuestion
{
    private protected DecisionQuestion(string id, IReadOnlyList<AIContent> instructions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(instructions);
        if (instructions.Count == 0)
        {
            throw new ArgumentException(
                "At least one instruction item is required.",
                nameof(instructions)
            );
        }

        Id = id;
        Instructions = instructions;
    }

    public string Id { get; }

    public abstract DecisionKind Kind { get; }

    public IReadOnlyList<AIContent> Instructions { get; }

    internal static string FormatText(IReadOnlyList<AIContent> contents)
    {
        return string.Join(
            Environment.NewLine,
            contents.OfType<TextContent>().Select(static c => c.Text)
        );
    }

    internal static void ValidateUniqueIds<T>(
        IReadOnlyList<T> items,
        Func<T, string> getId,
        string emptyMessage,
        string duplicateLabel,
        string paramName
    )
    {
        if (items is null)
        {
            throw new ArgumentNullException(paramName);
        }

        if (items.Count == 0)
        {
            throw new ArgumentException(emptyMessage, paramName);
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (item is null)
            {
                throw new ArgumentNullException(paramName);
            }
            var id = getId(item);
            if (!seenIds.Add(id))
            {
                throw new ArgumentException($"Duplicate {duplicateLabel} ID '{id}'.", paramName);
            }
        }
    }
}

public sealed class BinaryDecisionQuestion : DecisionQuestion
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

    public BinaryDecisionQuestion(
        string id,
        IReadOnlyList<AIContent> instructions,
        IReadOnlyList<AIContent>? trueDescription = null,
        IReadOnlyList<AIContent>? falseDescription = null
    )
        : base(id, instructions)
    {
        TrueDescription = trueDescription;
        FalseDescription = falseDescription;
    }

    public override DecisionKind Kind => DecisionKind.Binary;

    public IReadOnlyList<AIContent>? TrueDescription { get; }

    public IReadOnlyList<AIContent>? FalseDescription { get; }
}

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

    public ChoiceDecisionQuestion(
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

    public ScoreDecisionQuestion(
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

public sealed class DecisionCandidate
{
    public DecisionCandidate(string id, string description)
        : this(id, [new TextContent(description)])
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
    }

    public DecisionCandidate(string id, IReadOnlyList<AIContent> description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(description);
        if (description.Count == 0)
        {
            throw new ArgumentException(
                "At least one description item is required.",
                nameof(description)
            );
        }

        Id = id;
        Description = description;
    }

    public string Id { get; }

    public IReadOnlyList<AIContent> Description { get; }
}

public sealed class DecisionScoreLevel
{
    public DecisionScoreLevel(string id, string description)
        : this(id, [new TextContent(description)])
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
    }

    public DecisionScoreLevel(string id, IReadOnlyList<AIContent> description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(description);
        if (description.Count == 0)
        {
            throw new ArgumentException(
                "At least one description item is required.",
                nameof(description)
            );
        }

        Id = id;
        Description = description;
    }

    public string Id { get; }

    public IReadOnlyList<AIContent> Description { get; }
}
