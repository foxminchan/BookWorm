using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using Azure.AI.Projects;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Chassis.AI.Decisions;

internal sealed partial class FoundrySystemOneDecisionClient : IDecisionClient
{
    public const string ActivitySourceName = "BookWorm.Chassis.AI.Decisions";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly ClientPipeline _pipeline;
    private readonly Uri _systemOneEndpoint;
    private readonly string _defaultModelId;
    private readonly DecisionClientMetadata _metadata;
    private readonly ILogger<FoundrySystemOneDecisionClient> _logger;
    private bool _disposed;

    public FoundrySystemOneDecisionClient(
        AIProjectClient projectClient,
        string defaultModelId,
        ILogger<FoundrySystemOneDecisionClient>? logger = null
    )
    {
        ArgumentNullException.ThrowIfNull(projectClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultModelId);

        var projectEndpoint = projectClient.ProjectOpenAIClient.Endpoint
            ?? throw new InvalidOperationException(
                "AIProjectClient does not expose a valid endpoint URI."
            );

        _pipeline = projectClient.Pipeline;
        _systemOneEndpoint = new UriBuilder(projectEndpoint)
        {
            Path = "/providers/microsoft/v1/systemone",
            Query = string.Empty,
            Fragment = string.Empty,
        }.Uri;
        _defaultModelId = defaultModelId;
        _logger = logger ?? NullLogger<FoundrySystemOneDecisionClient>.Instance;
        _metadata = new DecisionClientMetadata(
            providerName: "microsoft.foundry",
            providerUri: _systemOneEndpoint,
            defaultModelId: _defaultModelId,
            supportedKinds: [DecisionKind.Binary, DecisionKind.Choice, DecisionKind.Score]
        );
    }

    public async Task<DecisionResponse> GetDecisionAsync(
        DecisionRequest request,
        DecisionOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        var modelId = string.IsNullOrWhiteSpace(options?.ModelId)
            ? _defaultModelId
            : options.ModelId;

        using var activity = ActivitySource.StartActivity("decision.systemone");
        activity?.SetTag("gen_ai.system", "microsoft.foundry");
        activity?.SetTag("gen_ai.request.model", modelId);
        activity?.SetTag("decision.question_count", request.Questions.Count);

        var payload = SystemOneProtocolMapper.BuildPayload(modelId, request);
        var raw = await PostJsonAsync(payload, activity, cancellationToken).ConfigureAwait(false);

        UsageDetails? usage = null;
        if (raw.Usage is not null)
        {
            activity?.SetTag("gen_ai.usage.input_tokens", raw.Usage.InputTokens);
            activity?.SetTag("gen_ai.usage.output_tokens", raw.Usage.OutputTokens);
            usage = new UsageDetails
            {
                InputTokenCount = raw.Usage.InputTokens,
                OutputTokenCount = raw.Usage.OutputTokens,
                TotalTokenCount = raw.Usage.InputTokens + raw.Usage.OutputTokens,
            };
        }

        return new DecisionResponse(
            SystemOneProtocolMapper.ParseAnswers(request.Questions, raw.Answers),
            raw.Model ?? modelId
        )
        {
            Usage = usage,
            RawRepresentation = raw,
        };
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        if (serviceKey is not null)
        {
            return null;
        }

        if (serviceType == typeof(DecisionClientMetadata))
        {
            return _metadata;
        }

        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private async Task<SystemOneResponse> PostJsonAsync(
        SystemOneRequest payload,
        Activity? activity,
        CancellationToken cancellationToken
    )
    {
        using var message = _pipeline.CreateMessage();
        message.Apply(new RequestOptions { CancellationToken = cancellationToken });
        message.Request.Method = HttpMethod.Post.Method;
        message.Request.Uri = _systemOneEndpoint;
        message.Request.Headers.Set("Accept", MediaTypeNames.Application.Json);
        message.Request.Headers.Set("Content-Type", MediaTypeNames.Application.Json);
        message.Request.Content = BinaryContent.Create(BinaryData.FromObjectAsJson(payload));

        try
        {
            await _pipeline.SendAsync(message).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new DecisionClientException(
                $"Failed to call Microsoft-Decision-1 endpoint '{_systemOneEndpoint}'.",
                ex,
                isTransient: ex is HttpRequestException
            );
        }

        var response = message.Response
            ?? throw new InvalidOperationException("Microsoft-Decision-1 returned no response.");

        if (response.IsError)
        {
            var errorBody = response.Content.ToString();
            LogRequestFailed(_logger, _systemOneEndpoint, response.Status, errorBody);
            activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {response.Status}");

            throw new DecisionClientException(
                $"Microsoft-Decision-1 request failed with HTTP {response.Status}: {errorBody}",
                new ClientResultException(response),
                isTransient: response.Status is 408 or 429 or (>= 500 and <= 599)
            );
        }

        try
        {
            return response.Content.ToObjectFromJson<SystemOneResponse>()
                ?? throw new InvalidOperationException(
                    "Microsoft-Decision-1 returned an empty response payload."
                );
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Microsoft-Decision-1 returned an invalid JSON response payload.",
                ex
            );
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Microsoft-Decision-1 request to {Endpoint} failed with status {StatusCode}: {Error}"
    )]
    private static partial void LogRequestFailed(
        ILogger logger,
        Uri endpoint,
        int statusCode,
        string error
    );
}
