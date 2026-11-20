using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions;

internal static class SystemOneProtocolMapper
{
    public static SystemOneRequest BuildPayload(string modelId, DecisionRequest request)
    {
        var combinedState = BuildCombinedState(request);

        var questions = request.Questions.ToDictionary(
            q => q.Id,
            q =>
                q switch
                {
                    BinaryDecisionQuestion binary => new SystemOneQuestion(
                        "noul",
                        FormatQuestionText(binary.Instructions),
                        null
                    ),
                    ChoiceDecisionQuestion choice => new SystemOneQuestion(
                        "choice",
                        FormatQuestionText(choice.Instructions),
                        choice.Candidates.ToDictionary(
                            c => c.Id,
                            c => FormatQuestionText(c.Description),
                            StringComparer.Ordinal
                        )
                    ),
                    ScoreDecisionQuestion score => new SystemOneQuestion(
                        "choice",
                        FormatQuestionText(score.Instructions),
                        score.Levels.ToDictionary(
                            l => l.Id,
                            l => FormatQuestionText(l.Description),
                            StringComparer.Ordinal
                        )
                    ),
                    _ => throw new NotSupportedException(
                        $"Unsupported decision question type '{q.GetType().Name}'."
                    ),
                },
            StringComparer.Ordinal
        );

        return new SystemOneRequest(modelId, combinedState, questions);
    }

    public static List<DecisionAnswer> ParseAnswers(
        IReadOnlyList<DecisionQuestion> questions,
        Dictionary<string, JsonElement>? rawAnswers
    )
    {
        var answers = new List<DecisionAnswer>(questions.Count);

        foreach (var question in questions)
        {
            if (
                rawAnswers is null
                || !rawAnswers.TryGetValue(question.Id, out var element)
                || element.ValueKind != JsonValueKind.Object
            )
            {
                answers.Add(
                    new UnansweredDecisionAnswer(
                        question.Id,
                        question.Kind,
                        new DecisionUnansweredReason("missing")
                    )
                );
                continue;
            }

            if (element.TryGetProperty("unanswered", out var unanswered))
            {
                var reasonText = unanswered.GetString();
                var reason = string.IsNullOrWhiteSpace(reasonText)
                    ? DecisionUnansweredReason.Refusal
                    : new DecisionUnansweredReason(reasonText);

                answers.Add(new UnansweredDecisionAnswer(question.Id, question.Kind, reason));
                continue;
            }

            answers.Add(ParseQuestionAnswer(question, element));
        }

        return answers;
    }

    private static DecisionAnswer ParseQuestionAnswer(
        DecisionQuestion question,
        JsonElement element
    )
    {
        return question switch
        {
            BinaryDecisionQuestion => element.TryGetProperty("noul", out var noul)
            && noul.TryGetDouble(out var trueProb)
                ? new BinaryDecisionAnswer(question.Id, trueProb)
                : new UnansweredDecisionAnswer(
                    question.Id,
                    DecisionKind.Binary,
                    new DecisionUnansweredReason("invalid_noul")
                ),

            ChoiceDecisionQuestion => ParseChoiceAnswer(question.Id, element),

            ScoreDecisionQuestion scoreQuestion => ParseScoreAnswer(scoreQuestion, element),

            _ => throw new NotSupportedException(
                $"Unsupported decision question type '{question.GetType().Name}'."
            ),
        };
    }

    private static DecisionAnswer ParseChoiceAnswer(string questionId, JsonElement element)
    {
        var selected = element.TryGetProperty("choice", out var choice)
            ? choice.GetString() ?? string.Empty
            : string.Empty;
        var probs = ParseProbabilities(element);

        return string.IsNullOrWhiteSpace(selected)
            ? new UnansweredDecisionAnswer(
                questionId,
                DecisionKind.Choice,
                new DecisionUnansweredReason("invalid_choice")
            )
            : new ChoiceDecisionAnswer(questionId, selected, probs);
    }

    private static ScoreDecisionAnswer ParseScoreAnswer(
        ScoreDecisionQuestion scoreQuestion,
        JsonElement element
    )
    {
        var selectedLevel = element.TryGetProperty("choice", out var levelChoice)
            ? levelChoice.GetString()
            : null;
        var levelProbs = ParseProbabilities(element);

        double? resolvedScore = null;
        if (!string.IsNullOrWhiteSpace(selectedLevel))
        {
            for (var i = 0; i < scoreQuestion.Levels.Count; i++)
            {
                if (
                    string.Equals(
                        scoreQuestion.Levels[i].Id,
                        selectedLevel,
                        StringComparison.Ordinal
                    )
                )
                {
                    resolvedScore = i;
                    break;
                }
            }
        }

        return new ScoreDecisionAnswer(scoreQuestion.Id, resolvedScore, levelProbs);
    }

    private static string BuildCombinedState(DecisionRequest request)
    {
        var stateParts = new List<string>();

        if (request.State is { } stateElement && stateElement.ValueKind != JsonValueKind.Null)
        {
            stateParts.Add(
                stateElement.ValueKind == JsonValueKind.String
                    ? stateElement.GetString() ?? string.Empty
                    : stateElement.GetRawText()
            );
        }

        foreach (var content in request.Contents)
        {
            if (content is TextContent textContent)
            {
                if (!string.IsNullOrWhiteSpace(textContent.Text))
                {
                    stateParts.Add(textContent.Text);
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"Microsoft-Decision-1 does not support AIContent type '{content.GetType().Name}'."
                );
            }
        }

        return string.Join(
            Environment.NewLine,
            stateParts.Where(static s => !string.IsNullOrWhiteSpace(s))
        );
    }

    private static string FormatQuestionText(IReadOnlyList<AIContent> contents)
    {
        foreach (var content in contents)
        {
            if (content is not TextContent)
            {
                throw new NotSupportedException(
                    $"Microsoft-Decision-1 does not support AIContent type '{content.GetType().Name}' in question instructions or descriptions."
                );
            }
        }

        return DecisionQuestion.FormatText(contents);
    }

    private static List<DecisionProbability> ParseProbabilities(JsonElement element)
    {
        return
            element.TryGetProperty("probabilities", out var probsProp)
            && probsProp.ValueKind == JsonValueKind.Object
            ? probsProp
                .EnumerateObject()
                .Where(static p => p.Value.TryGetDouble(out _))
                .Select(static p => new DecisionProbability(p.Name, p.Value.GetDouble()))
                .ToList()
            : [];
    }
}

internal sealed record SystemOneRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("questions")] Dictionary<string, SystemOneQuestion> Questions
);

internal sealed record SystemOneQuestion(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("instructions")] string Instructions,
    [property: JsonPropertyName("criteria")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        Dictionary<string, string>? Criteria
);

internal sealed record SystemOneUsage(
    [property: JsonPropertyName("input_tokens")] int InputTokens,
    [property: JsonPropertyName("output_tokens")] int OutputTokens
);

internal sealed record SystemOneResponse(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("answers")] Dictionary<string, JsonElement>? Answers,
    [property: JsonPropertyName("usage")] SystemOneUsage? Usage
);
