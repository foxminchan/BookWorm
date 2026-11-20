using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookWorm.Chassis.AI.Decisions;

/// <summary>
///     Defines a provider-neutral client for decision-oriented AI models that evaluate
///     application state against bounded questions and return calibrated probabilities.
/// </summary>
/// <remarks>
///     Follows the <c>IDecisionClient</c> abstraction proposed in
///     <see href="https://github.com/dotnet/extensions/issues/7764">dotnet/extensions#7764</see>.
/// </remarks>
public interface IDecisionClient : IDisposable
{
    /// <summary>
    ///     Evaluates the supplied <paramref name="request" /> and returns structured probabilistic answers.
    /// </summary>
    Task<DecisionResponse> GetDecisionAsync(
        DecisionRequest request,
        DecisionOptions? options = null,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
///     Represents a batch of bounded decision questions evaluated against a shared application state.
/// </summary>
public sealed class DecisionRequest
{
    public DecisionRequest(JsonElement state, IReadOnlyList<DecisionQuestion> questions)
    {
        ArgumentNullException.ThrowIfNull(questions);
        if (questions.Count == 0)
        {
            throw new ArgumentException(
                "At least one decision question is required.",
                nameof(questions)
            );
        }

        State = state;
        Questions = questions;
    }

    public DecisionRequest(string state, IReadOnlyList<DecisionQuestion> questions)
        : this(JsonSerializer.SerializeToElement(state), questions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
    }

    /// <summary>
    ///     Gets the shared application state evaluated by every question in <see cref="Questions" />.
    /// </summary>
    public JsonElement State { get; }

    /// <summary>
    ///     Gets the bounded questions to evaluate.
    /// </summary>
    public IReadOnlyList<DecisionQuestion> Questions { get; }
}

/// <summary>
///     Base type for bounded decision questions.
/// </summary>
public abstract class DecisionQuestion
{
    protected DecisionQuestion(string id, string instructions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(instructions);

        Id = id;
        Instructions = instructions;
    }

    /// <summary>
    ///     Gets the unique identifier for correlating the question with its <see cref="DecisionAnswer" />.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Gets the evaluation instructions for this question.
    /// </summary>
    public string Instructions { get; }
}

/// <summary>
///     A yes/no (true/false) proposition question.
/// </summary>
public sealed class BinaryDecisionQuestion(string id, string instructions)
    : DecisionQuestion(id, instructions);

/// <summary>
///     A categorical selection question over a caller-defined set of candidates.
/// </summary>
public sealed class ChoiceDecisionQuestion : DecisionQuestion
{
    public ChoiceDecisionQuestion(
        string id,
        string instructions,
        IReadOnlyList<DecisionCandidate> candidates
    )
        : base(id, instructions)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (candidates.Count == 0)
        {
            throw new ArgumentException("At least one candidate is required.", nameof(candidates));
        }

        Candidates = candidates;
    }

    /// <summary>
    ///     Gets the allowed candidate choices.
    /// </summary>
    public IReadOnlyList<DecisionCandidate> Candidates { get; }
}

/// <summary>
///     An ordinal rubric scoring question over ordered score levels.
/// </summary>
public sealed class ScoreDecisionQuestion : DecisionQuestion
{
    public ScoreDecisionQuestion(
        string id,
        string instructions,
        IReadOnlyList<DecisionScoreLevel> levels
    )
        : base(id, instructions)
    {
        ArgumentNullException.ThrowIfNull(levels);
        if (levels.Count == 0)
        {
            throw new ArgumentException("At least one score level is required.", nameof(levels));
        }

        Levels = levels;
    }

    /// <summary>
    ///     Gets the ordered scoring levels.
    /// </summary>
    public IReadOnlyList<DecisionScoreLevel> Levels { get; }
}

/// <summary>
///     Represents a candidate option in a <see cref="ChoiceDecisionQuestion" />.
/// </summary>
public sealed record DecisionCandidate(string Id, string Description);

/// <summary>
///     Represents an ordinal level in a <see cref="ScoreDecisionQuestion" />.
/// </summary>
public sealed record DecisionScoreLevel(string Id, string Description);

/// <summary>
///     Represents a single probability entry for a candidate or score level.
/// </summary>
public sealed record DecisionProbability(string Id, double Probability);

/// <summary>
///     Base type for answers returned by an <see cref="IDecisionClient" />.
/// </summary>
public abstract class DecisionAnswer
{
    protected DecisionAnswer(string questionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionId);
        QuestionId = questionId;
    }

    /// <summary>
    ///     Gets the identifier of the <see cref="DecisionQuestion" /> this answer corresponds to.
    /// </summary>
    public string QuestionId { get; }
}

/// <summary>
///     Answer for a <see cref="BinaryDecisionQuestion" />.
/// </summary>
public sealed class BinaryDecisionAnswer(string questionId, double trueProbability)
    : DecisionAnswer(questionId)
{
    /// <summary>
    ///     Gets the calibrated probability in <c>[0, 1]</c> that the proposition is true.
    /// </summary>
    public double TrueProbability { get; } = trueProbability;
}

/// <summary>
///     Answer for a <see cref="ChoiceDecisionQuestion" />.
/// </summary>
public sealed class ChoiceDecisionAnswer(
    string questionId,
    string selectedCandidateId,
    double confidence,
    IReadOnlyList<DecisionProbability> probabilities
) : DecisionAnswer(questionId)
{
    /// <summary>
    ///     Gets the selected candidate identifier.
    /// </summary>
    public string SelectedCandidateId { get; } = selectedCandidateId;

    /// <summary>
    ///     Gets the confidence score for the selected candidate.
    /// </summary>
    public double Confidence { get; } = confidence;

    /// <summary>
    ///     Gets the probability distribution across the supplied candidates.
    /// </summary>
    public IReadOnlyList<DecisionProbability> Probabilities { get; } = probabilities;
}

/// <summary>
///     Answer for a <see cref="ScoreDecisionQuestion" />.
/// </summary>
public sealed class ScoreDecisionAnswer(
    string questionId,
    double score,
    double confidence,
    IReadOnlyList<DecisionProbability> probabilities
) : DecisionAnswer(questionId)
{
    /// <summary>
    ///     Gets the probability-weighted score reported by the model.
    /// </summary>
    public double Score { get; } = score;

    /// <summary>
    ///     Gets the confidence score for the evaluation.
    /// </summary>
    public double Confidence { get; } = confidence;

    /// <summary>
    ///     Gets the probability distribution across the supplied score levels.
    /// </summary>
    public IReadOnlyList<DecisionProbability> Probabilities { get; } = probabilities;
}

/// <summary>
///     Represents a question that the provider declined or was unable to answer.
/// </summary>
public sealed class UnansweredDecisionAnswer(string questionId, string? reason = null)
    : DecisionAnswer(questionId)
{
    /// <summary>
    ///     Gets the optional reason the question was unanswered.
    /// </summary>
    public string? Reason { get; } = reason;
}

/// <summary>
///     Token usage details reported by a decision model.
/// </summary>
public sealed record DecisionUsage(
    [property: JsonPropertyName("input_tokens")] int InputTokens,
    [property: JsonPropertyName("output_tokens")] int OutputTokens
);

/// <summary>
///     Represents the response from an <see cref="IDecisionClient" /> call.
/// </summary>
public sealed class DecisionResponse(
    IReadOnlyList<DecisionAnswer> answers,
    string? modelId = null,
    DecisionUsage? usage = null
)
{
    /// <summary>
    ///     Gets all answers keyed to the requested <see cref="DecisionQuestion.Id" /> values.
    /// </summary>
    public IReadOnlyList<DecisionAnswer> Answers { get; } = answers;

    /// <summary>
    ///     Gets the model identifier that produced the response.
    /// </summary>
    public string? ModelId { get; } = modelId;

    /// <summary>
    ///     Gets optional token usage metrics.
    /// </summary>
    public DecisionUsage? Usage { get; } = usage;

    /// <summary>
    ///     Finds the <see cref="DecisionAnswer" /> for the given <paramref name="questionId" />.
    /// </summary>
    public DecisionAnswer? GetAnswer(string questionId)
    {
        return Answers.FirstOrDefault(a =>
            string.Equals(a.QuestionId, questionId, StringComparison.Ordinal)
        );
    }

    /// <summary>
    ///     Gets the <see cref="BinaryDecisionAnswer" /> for <paramref name="questionId" />, if present.
    /// </summary>
    public BinaryDecisionAnswer? GetBinaryAnswer(string questionId)
    {
        return GetAnswer(questionId) as BinaryDecisionAnswer;
    }

    /// <summary>
    ///     Gets the <see cref="ChoiceDecisionAnswer" /> for <paramref name="questionId" />, if present.
    /// </summary>
    public ChoiceDecisionAnswer? GetChoiceAnswer(string questionId)
    {
        return GetAnswer(questionId) as ChoiceDecisionAnswer;
    }

    /// <summary>
    ///     Gets the <see cref="ScoreDecisionAnswer" /> for <paramref name="questionId" />, if present.
    /// </summary>
    public ScoreDecisionAnswer? GetScoreAnswer(string questionId)
    {
        return GetAnswer(questionId) as ScoreDecisionAnswer;
    }
}

/// <summary>
///     Options controlling a single <see cref="IDecisionClient.GetDecisionAsync" /> invocation.
/// </summary>
public sealed class DecisionOptions
{
    /// <summary>
    ///     Gets or sets the target model or deployment name override.
    /// </summary>
    public string? ModelId { get; set; }
}

/// <summary>
///     Convenience extensions for <see cref="IDecisionClient" />.
/// </summary>
public static class DecisionClientExtensions
{
    extension(IDecisionClient client)
    {
        /// <summary>
        ///     Serializes <paramref name="state" /> to JSON and evaluates <paramref name="questions" />.
        /// </summary>
        public Task<DecisionResponse> GetDecisionAsync<TState>(
            TState state,
            IReadOnlyList<DecisionQuestion> questions,
            DecisionOptions? options = null,
            JsonSerializerOptions? serializerOptions = null,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(client);
            ArgumentNullException.ThrowIfNull(state);

            var stateElement = state is JsonElement element
                ? element
                : JsonSerializer.SerializeToElement(state, serializerOptions);

            return client.GetDecisionAsync(
                new DecisionRequest(stateElement, questions),
                options,
                cancellationToken
            );
        }
    }
}
