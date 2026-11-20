using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions.Questions;

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
