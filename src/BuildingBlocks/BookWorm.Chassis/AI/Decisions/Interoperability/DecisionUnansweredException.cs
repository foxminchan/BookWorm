namespace BookWorm.Chassis.AI.Decisions;

public sealed class DecisionUnansweredException : InvalidOperationException
{
    public DecisionUnansweredException(DecisionResponse response, IEnumerable<string> questionIds)
        : base(BuildMessage(questionIds))
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(questionIds);

        Response = response;
        QuestionIds = questionIds.ToList();
    }

    public DecisionResponse Response { get; }

    public IReadOnlyList<string> QuestionIds { get; }

    private static string BuildMessage(IEnumerable<string> questionIds)
    {
        return $"Decision questions were unanswered: {string.Join(", ", questionIds)}.";
    }
}
