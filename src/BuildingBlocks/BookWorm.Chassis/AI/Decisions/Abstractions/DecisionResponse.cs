using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions;

public sealed class DecisionResponse
{
    public DecisionResponse()
    {
        Answers = [];
    }

    public DecisionResponse(IList<DecisionAnswer> answers, string? modelId = null)
    {
        ArgumentNullException.ThrowIfNull(answers);
        Answers = answers;
        ModelId = modelId;
    }

    public IList<DecisionAnswer> Answers { get; set; }

    public string? ModelId { get; set; }

    public string? ResponseId { get; set; }

    public UsageDetails? Usage { get; set; }

    public object? RawRepresentation { get; set; }

    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    public DecisionAnswer? GetAnswer(string questionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        return Answers.FirstOrDefault(a =>
            string.Equals(a.QuestionId, questionId, StringComparison.Ordinal)
        );
    }

    public BinaryDecisionAnswer? GetBinaryAnswer(string questionId)
    {
        return GetAnswer(questionId) as BinaryDecisionAnswer;
    }

    public ChoiceDecisionAnswer? GetChoiceAnswer(string questionId)
    {
        return GetAnswer(questionId) as ChoiceDecisionAnswer;
    }

    public ScoreDecisionAnswer? GetScoreAnswer(string questionId)
    {
        return GetAnswer(questionId) as ScoreDecisionAnswer;
    }
}
