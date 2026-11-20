using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Questions;

public sealed class DecisionScoreLevel
{
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
