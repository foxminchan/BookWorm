using System.Data.Common;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Decisions;

/// <summary>
///     <see cref="IDecisionClient" /> implementation for Microsoft Foundry's
///     <c>Microsoft-Decision-1</c> (<c>/providers/microsoft/v1/systemone</c>) endpoint.
/// </summary>
public sealed class FoundrySystemOneDecisionClient : IDecisionClient
{
    public const string ActivitySourceName = "BookWorm.Chassis.AI.Decisions";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly TokenRequestContext FoundryTokenContext = new([
        "https://ai.azure.com/.default",
    ]);

    private readonly HttpClient _httpClient;
    private readonly Uri _systemOneEndpoint;
    private readonly string _defaultModelId;
    private readonly string? _apiKey;
    private readonly TokenCredential? _credential;
    private readonly ILogger<FoundrySystemOneDecisionClient>? _logger;

    public FoundrySystemOneDecisionClient(
        HttpClient httpClient,
        Uri foundryEndpoint,
        string defaultModelId,
        string? apiKey = null,
        TokenCredential? credential = null,
        ILogger<FoundrySystemOneDecisionClient>? logger = null
    )
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(foundryEndpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultModelId);

        _httpClient = httpClient;
        _systemOneEndpoint = BuildSystemOneEndpoint(foundryEndpoint);
        _defaultModelId = defaultModelId;
        _apiKey = apiKey;
        _credential = credential;
        _logger = logger;
    }

    public async Task<DecisionResponse> GetDecisionAsync(
        DecisionRequest request,
        DecisionOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var modelId = string.IsNullOrWhiteSpace(options?.ModelId)
            ? _defaultModelId
            : options.ModelId;

        using var activity = ActivitySource.StartActivity("decision.systemone");
        activity?.SetTag("gen_ai.system", "microsoft.foundry");
        activity?.SetTag("gen_ai.request.model", modelId);
        activity?.SetTag("decision.question_count", request.Questions.Count);

        var payload = BuildPayload(modelId, request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _systemOneEndpoint)
        {
            Content = JsonContent.Create(payload),
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            httpRequest.Headers.Add("api-key", _apiKey);
        }
        else if (_credential is not null)
        {
            var token = await _credential
                .GetTokenAsync(FoundryTokenContext, cancellationToken)
                .ConfigureAwait(false);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.Token
            );
        }
        else
        {
            throw new InvalidOperationException(
                "Foundry System One client requires either an API key or a TokenCredential."
            );
        }

        using var response = await _httpClient
            .SendAsync(httpRequest, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response
                .Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger?.LogWarning(
                "Microsoft-Decision-1 request to {Endpoint} failed with status {StatusCode}: {Error}",
                _systemOneEndpoint,
                (int)response.StatusCode,
                errorBody
            );

            activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {(int)response.StatusCode}");

            throw new HttpRequestException(
                $"Microsoft-Decision-1 request failed with status {(int)response.StatusCode}: {errorBody}",
                null,
                response.StatusCode
            );
        }

        var raw = await response
            .Content.ReadFromJsonAsync<SystemOneResponse>(cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException(
                "Microsoft-Decision-1 returned an empty response payload."
            );

        if (raw.Usage is not null)
        {
            activity?.SetTag("gen_ai.usage.input_tokens", raw.Usage.InputTokens);
            activity?.SetTag("gen_ai.usage.output_tokens", raw.Usage.OutputTokens);
        }

        var answers = ParseAnswers(request.Questions, raw.Answers);

        return new DecisionResponse(answers, raw.Model ?? modelId, raw.Usage);
    }

    public void Dispose()
    {
        // HttpClient lifetime is managed by IHttpClientFactory.
    }

    public static (Uri Endpoint, string? ApiKey) ParseConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var directUri))
        {
            return (directUri, null);
        }

        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        if (
            !builder.TryGetValue("Endpoint", out var endpointObj)
            || endpointObj is not string endpointStr
            || !Uri.TryCreate(endpointStr, UriKind.Absolute, out var endpoint)
        )
        {
            throw new InvalidOperationException(
                "Foundry connection string must contain a valid 'Endpoint' URI."
            );
        }

        var apiKey = builder.TryGetValue("Key", out var keyObj) ? keyObj as string : null;

        return (endpoint, apiKey);
    }

    public static Uri BuildSystemOneEndpoint(Uri foundryEndpoint)
    {
        var root = new UriBuilder(foundryEndpoint)
        {
            Path = "/providers/microsoft/v1/systemone",
            Query = string.Empty,
            Fragment = string.Empty,
        };

        return root.Uri;
    }

    private static SystemOneRequest BuildPayload(string modelId, DecisionRequest request)
    {
        var questions = new Dictionary<string, SystemOneQuestion>(
            request.Questions.Count,
            StringComparer.Ordinal
        );

        foreach (var question in request.Questions)
        {
            questions[question.Id] = question switch
            {
                BinaryDecisionQuestion binary => new SystemOneQuestion(
                    "noul",
                    binary.Instructions,
                    null
                ),
                ChoiceDecisionQuestion choice => new SystemOneQuestion(
                    "choice",
                    choice.Instructions,
                    choice.Candidates.ToDictionary(
                        c => c.Id,
                        c => c.Description,
                        StringComparer.Ordinal
                    )
                ),
                ScoreDecisionQuestion score => new SystemOneQuestion(
                    "score",
                    score.Instructions,
                    score.Levels.ToDictionary(l => l.Id, l => l.Description, StringComparer.Ordinal)
                ),
                _ => throw new NotSupportedException(
                    $"Unsupported decision question type '{question.GetType().Name}'."
                ),
            };
        }

        return new SystemOneRequest(modelId, request.State, questions);
    }

    private static IReadOnlyList<DecisionAnswer> ParseAnswers(
        IReadOnlyList<DecisionQuestion> questions,
        Dictionary<string, JsonElement>? rawAnswers
    )
    {
        var answers = new List<DecisionAnswer>(questions.Count);

        foreach (var question in questions)
        {
            if (
                rawAnswers is null
                || !rawAnswers.TryGetValue(question.Id, out var answerElement)
                || answerElement.ValueKind != JsonValueKind.Object
            )
            {
                answers.Add(new UnansweredDecisionAnswer(question.Id, "missing"));
                continue;
            }

            if (answerElement.TryGetProperty("unanswered", out var unansweredProp))
            {
                answers.Add(new UnansweredDecisionAnswer(question.Id, unansweredProp.GetString()));
                continue;
            }

            switch (question)
            {
                case BinaryDecisionQuestion:
                    if (
                        answerElement.TryGetProperty("noul", out var noulProp)
                        && noulProp.TryGetDouble(out var trueProbability)
                    )
                    {
                        answers.Add(new BinaryDecisionAnswer(question.Id, trueProbability));
                    }
                    else
                    {
                        answers.Add(new UnansweredDecisionAnswer(question.Id, "invalid_noul"));
                    }
                    break;

                case ChoiceDecisionQuestion:
                    var selected = answerElement.TryGetProperty("choice", out var choiceProp)
                        ? choiceProp.GetString() ?? string.Empty
                        : string.Empty;
                    var choiceConfidence =
                        answerElement.TryGetProperty("confidence", out var confProp)
                        && confProp.TryGetDouble(out var c)
                            ? c
                            : 0.0;
                    var choiceProbabilities = ParseProbabilities(answerElement);
                    answers.Add(
                        new ChoiceDecisionAnswer(
                            question.Id,
                            selected,
                            choiceConfidence,
                            choiceProbabilities
                        )
                    );
                    break;

                case ScoreDecisionQuestion:
                    var score =
                        answerElement.TryGetProperty("score", out var scoreProp)
                        && scoreProp.TryGetDouble(out var s)
                            ? s
                            : 0.0;
                    var scoreConfidence =
                        answerElement.TryGetProperty("confidence", out var scoreConfProp)
                        && scoreConfProp.TryGetDouble(out var sc)
                            ? sc
                            : 0.0;
                    var scoreProbabilities = ParseProbabilities(answerElement);
                    answers.Add(
                        new ScoreDecisionAnswer(
                            question.Id,
                            score,
                            scoreConfidence,
                            scoreProbabilities
                        )
                    );
                    break;
            }
        }

        return answers;
    }

    private static IReadOnlyList<DecisionProbability> ParseProbabilities(JsonElement answerElement)
    {
        if (
            !answerElement.TryGetProperty("probabilities", out var probsProp)
            || probsProp.ValueKind != JsonValueKind.Object
        )
        {
            return [];
        }

        var list = new List<DecisionProbability>();
        foreach (var prop in probsProp.EnumerateObject())
        {
            if (prop.Value.TryGetDouble(out var val))
            {
                list.Add(new DecisionProbability(prop.Name, val));
            }
        }

        return list;
    }

    private sealed record SystemOneRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("state")] JsonElement State,
        [property: JsonPropertyName("questions")] Dictionary<string, SystemOneQuestion> Questions
    );

    private sealed record SystemOneQuestion(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("instructions")] string Instructions,
        [property: JsonPropertyName("criteria")]
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            Dictionary<string, string>? Criteria
    );

    private sealed record SystemOneResponse(
        [property: JsonPropertyName("model")] string? Model,
        [property: JsonPropertyName("answers")] Dictionary<string, JsonElement>? Answers,
        [property: JsonPropertyName("usage")] DecisionUsage? Usage
    );
}
