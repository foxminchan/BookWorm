using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace BookWorm.Chassis.AI.Decisions;

public sealed partial class DecisionDefinition<TResult>
{
    public sealed class Builder
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly JsonTypeInfo<TResult> _resultTypeInfo;
        private readonly HashSet<string> _seenQuestionIds = new(StringComparer.Ordinal);

        internal Builder(
            JsonSerializerOptions serializerOptions,
            JsonTypeInfo<TResult> resultTypeInfo
        )
        {
            _serializerOptions = serializerOptions;
            _resultTypeInfo = resultTypeInfo;
        }

        internal List<BoundQuestion> Bindings { get; } = [];

        public void Choice<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum
        >(Expression<Func<TResult, TEnum>> selector, string? instructions = null)
            where TEnum : struct, Enum
        {
            var questionId = RegisterQuestionId(selector, out var member);
            var resolvedInstructions = ResolveInstructions(instructions, member);
            var enumDefinition = new DecisionEnumDefinition<TEnum>(_serializerOptions);
            var question = enumDefinition.CreateQuestion(questionId, resolvedInstructions);

            Bindings.Add(new BoundQuestion(question, (answer, jsonObject, distributions) =>
                {
                    if (answer is not ChoiceDecisionAnswer choiceAnswer)
                    {
                        throw new InvalidOperationException(
                            $"Expected ChoiceDecisionAnswer for '{questionId}'."
                        );
                    }

                    var candidateIds = new HashSet<string>(
                        question.Candidates.Select(static c => c.Id),
                        StringComparer.Ordinal
                    );
                    if (!candidateIds.Contains(choiceAnswer.SelectedCandidateId))
                    {
                        throw new InvalidOperationException(
                            $"Selected candidate '{choiceAnswer.SelectedCandidateId}' is not declared on question '{questionId}'."
                        );
                    }

                    var distribution = new Dictionary<TEnum, double>();
                    foreach (var probability in choiceAnswer.Probabilities)
                    {
                        if (!candidateIds.Contains(probability.Id))
                        {
                            throw new InvalidOperationException(
                                $"Probability candidate '{probability.Id}' is not declared on question '{questionId}'."
                            );
                        }

                        if (enumDefinition.TryParse(probability.Id, out var enumValue))
                        {
                            distribution[enumValue] = probability.Value;
                        }
                    }

                    var parsed = enumDefinition.Parse(choiceAnswer.SelectedCandidateId);
                    jsonObject[questionId] = JsonSerializer.SerializeToNode(
                        parsed,
                        _serializerOptions
                    );
                    distributions[questionId] = distribution;
                }));
        }

        public void BinaryProbability(
            Expression<Func<TResult, double>> selector,
            string? instructions = null
        )
        {
            var questionId = RegisterQuestionId(selector, out var member);
            var resolvedInstructions = ResolveInstructions(instructions, member);
            var question = new BinaryDecisionQuestion(questionId, resolvedInstructions);

            Bindings.Add(new BoundQuestion(question, (answer, jsonObject, _) =>
                {
                    if (answer is not BinaryDecisionAnswer binaryAnswer)
                    {
                        throw new InvalidOperationException(
                            $"Expected BinaryDecisionAnswer for '{questionId}'."
                        );
                    }

                    jsonObject[questionId] = binaryAnswer.TrueProbability;
                }));
        }

        public void Score(
            Expression<Func<TResult, double>> selector,
            IReadOnlyList<DecisionScoreLevel> levels,
            string? instructions = null
        )
        {
            ArgumentNullException.ThrowIfNull(levels);
            var questionId = RegisterQuestionId(selector, out var member);
            var resolvedInstructions = ResolveInstructions(instructions, member);
            var question = new ScoreDecisionQuestion(questionId, resolvedInstructions, levels);

            Bindings.Add(new BoundQuestion(question, (answer, jsonObject, _) =>
                {
                    var scoreAnswer = ValidateScoreAnswer(question, answer);
                    if (scoreAnswer.Score is not { } score)
                    {
                        throw new InvalidOperationException(
                            $"Provider did not report a score for question '{questionId}'."
                        );
                    }

                    if (score > question.Levels.Count - 1)
                    {
                        throw new InvalidOperationException(
                            $"Reported score {score} exceeds highest level index {question.Levels.Count - 1} for question '{questionId}'."
                        );
                    }

                    jsonObject[questionId] = score;
                }));
        }

        public void ExpectedScore(
            Expression<Func<TResult, double>> selector,
            IReadOnlyList<DecisionScoreLevel> levels,
            string? instructions = null
        )
        {
            ArgumentNullException.ThrowIfNull(levels);
            var questionId = RegisterQuestionId(selector, out var member);
            var resolvedInstructions = ResolveInstructions(instructions, member);
            var question = new ScoreDecisionQuestion(questionId, resolvedInstructions, levels);

            Bindings.Add(new BoundQuestion(question, (answer, jsonObject, _) =>
                {
                    var scoreAnswer = ValidateScoreAnswer(question, answer);
                    var expected = scoreAnswer.GetExpectedScore(question)
                        ?? throw new InvalidOperationException(
                            $"Score distribution has no positive probability for question '{questionId}'."
                        );

                    jsonObject[questionId] = expected;
                }));
        }

        private string RegisterQuestionId(LambdaExpression selector, out MemberInfo member)
        {
            member = ExtractMember(selector);
            var questionId = ResolvePropertyJsonName(selector, _resultTypeInfo, _serializerOptions);
            if (!_seenQuestionIds.Add(questionId))
            {
                throw new ArgumentException(
                    $"Property '{questionId}' is already bound in this DecisionDefinition.",
                    nameof(selector)
                );
            }

            return questionId;
        }

        private static string ResolveInstructions(string? instructions, MemberInfo member)
        {
            return string.IsNullOrWhiteSpace(instructions)
                ? member.GetCustomAttribute<DescriptionAttribute>()?.Description ?? member.Name
                : instructions;
        }

        private static ScoreDecisionAnswer ValidateScoreAnswer(
            ScoreDecisionQuestion question,
            DecisionAnswer answer
        )
        {
            if (answer is not ScoreDecisionAnswer scoreAnswer)
            {
                throw new InvalidOperationException(
                    $"Expected ScoreDecisionAnswer for '{question.Id}'."
                );
            }

            var levelIds = new HashSet<string>(
                question.Levels.Select(static l => l.Id),
                StringComparer.Ordinal
            );
            foreach (var probability in scoreAnswer.Probabilities)
            {
                if (!levelIds.Contains(probability.Id))
                {
                    throw new InvalidOperationException(
                        $"Probability level '{probability.Id}' is not declared on question '{question.Id}'."
                    );
                }
            }

            return scoreAnswer;
        }
    }
}
