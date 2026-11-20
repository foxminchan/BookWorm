using BookWorm.Chassis.AI.Decisions.Abstractions;

namespace BookWorm.Chassis.AI.Decisions.Interoperability;

public sealed class DecisionUnansweredException : InvalidOperationException
{
    public DecisionUnansweredException(DecisionResponse response, IEnumerable<string> questionIds)
        : this(response, questionIds as IReadOnlyList<string> ?? [.. questionIds]) { }

    private DecisionUnansweredException(
        DecisionResponse response,
        IReadOnlyList<string> questionIds
    )
        : base(BuildMessage(questionIds))
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(questionIds);

        Response = response;
        QuestionIds = questionIds;
    }

    public DecisionResponse Response { get; }

    public IReadOnlyList<string> QuestionIds { get; }

    private static string BuildMessage(IEnumerable<string> questionIds)
    {
        return $"Decision questions were unanswered: {string.Join(", ", questionIds)}.";
    }
}
