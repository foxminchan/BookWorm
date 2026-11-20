using System.ComponentModel;
using System.Text.Json;
using BookWorm.Chassis.AI.Decisions.Abstractions.Questions;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions;

public enum DecisionKind : byte
{
    [Description("A yes/no (true/false) proposition decision.")]
    Binary = 0,

    [Description("A categorical selection decision over discrete candidates.")]
    Choice = 1,

    [Description("An ordered rubric or rating decision over score levels.")]
    Score = 2,
}

public sealed class DecisionRequest
{
    public DecisionRequest(
        JsonElement? state,
        IReadOnlyList<DecisionQuestion> questions,
        IReadOnlyList<AIContent>? contents = null
    )
    {
        ArgumentNullException.ThrowIfNull(questions);
        if (questions.Count == 0)
        {
            throw new ArgumentException(
                "At least one decision question is required.",
                nameof(questions)
            );
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var question in questions)
        {
            ArgumentNullException.ThrowIfNull(question);
            if (!seenIds.Add(question.Id))
            {
                throw new ArgumentException(
                    $"Duplicate decision question ID '{question.Id}'.",
                    nameof(questions)
                );
            }
        }

        State = state;
        Questions = questions;
        Contents = contents ?? [];
    }

    public JsonElement? State { get; }
    public IReadOnlyList<AIContent> Contents { get; }
    public IReadOnlyList<DecisionQuestion> Questions { get; }
}
