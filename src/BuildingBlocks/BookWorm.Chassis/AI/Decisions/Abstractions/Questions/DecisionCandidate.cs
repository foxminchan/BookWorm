using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Questions;

public sealed class DecisionCandidate
{
    public DecisionCandidate(string id, string description)
        : this(id, [new TextContent(description)])
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
    }

    private DecisionCandidate(string id, IReadOnlyList<AIContent> description)
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
