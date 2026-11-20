using System.Diagnostics.CodeAnalysis;

namespace BookWorm.Chassis.AI.Decisions;

public static class DecisionBindingExtensions
{
    extension(DecisionResponse response)
    {
        public TResult Bind<TResult>(Func<DecisionAnswerSet, TResult> map)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(map);

            return map(new DecisionAnswerSet(response));
        }
    }
}

public sealed class DecisionAnswerSet
{
    public DecisionAnswerSet(DecisionResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        Response = response;
    }

    public DecisionResponse Response { get; }

    public BinaryDecisionAnswer GetBinary(string questionId)
    {
        return GetRequiredAnswer<BinaryDecisionAnswer>(questionId, DecisionKind.Binary);
    }

    public ChoiceDecisionAnswer GetChoice(string questionId)
    {
        return GetRequiredAnswer<ChoiceDecisionAnswer>(questionId, DecisionKind.Choice);
    }

    public TEnum GetChoice<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum
    >(string questionId, DecisionEnumDefinition<TEnum> enumDefinition)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(enumDefinition);
        var answer = GetChoice(questionId);
        return enumDefinition.Parse(answer.SelectedCandidateId);
    }

    public ScoreDecisionAnswer GetScore(string questionId)
    {
        return GetRequiredAnswer<ScoreDecisionAnswer>(questionId, DecisionKind.Score);
    }

    private TAnswer GetRequiredAnswer<TAnswer>(string questionId, DecisionKind expectedKind)
        where TAnswer : DecisionAnswer
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);

        var answer = Response.GetAnswer(questionId)
            ?? throw new InvalidOperationException(
                $"No answer was returned for decision question '{questionId}'."
            );

        if (answer is UnansweredDecisionAnswer)
        {
            throw new DecisionUnansweredException(Response, [questionId]);
        }

        if (answer.Kind != expectedKind || answer is not TAnswer typedAnswer)
        {
            throw new InvalidOperationException(
                $"Decision question '{questionId}' expected answer kind '{expectedKind}' but received '{answer.Kind}'."
            );
        }

        return typedAnswer;
    }
}
