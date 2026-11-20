using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions.Abstractions;

public sealed class DecisionResponse
{
    public DecisionResponse(IList<DecisionAnswer> answers, string? modelId = null)
    {
        ArgumentNullException.ThrowIfNull(answers);
        Answers = answers;
        ModelId = modelId;
    }

    private IList<DecisionAnswer> Answers { get; }

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
}
