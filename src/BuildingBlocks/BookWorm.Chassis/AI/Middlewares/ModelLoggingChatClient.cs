using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

internal sealed partial class ModelLoggingChatClient(
    IChatClient innerClient,
    ILogger<ModelLoggingChatClient> logger
) : DelegatingChatClient(innerClient)
{
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await InnerClient.GetResponseAsync(messages, options, cancellationToken);

        LogModel(response.ModelId, response.ResponseId, GetRequestedModel(options), false);

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var requestedModel = GetRequestedModel(options);
        string? lastModelId = null;
        string? responseId = null;

        await foreach (
            var update in InnerClient.GetStreamingResponseAsync(
                messages,
                options,
                cancellationToken
            )
        )
        {
            responseId = update.ResponseId ?? responseId;

            if (
                !string.IsNullOrWhiteSpace(update.ModelId)
                && !string.Equals(lastModelId, update.ModelId, StringComparison.Ordinal)
            )
            {
                LogModel(update.ModelId, responseId, requestedModel, true);
                lastModelId = update.ModelId;
            }

            yield return update;
        }

        if (lastModelId is null)
        {
            LogModel(null, responseId, requestedModel, true);
        }
    }

    private string? GetRequestedModel(ChatOptions? options)
    {
        return options?.ModelId ?? InnerClient.GetService<ChatClientMetadata>()?.DefaultModelId;
    }

    private void LogModel(
        string? modelId,
        string? responseId,
        string? requestedModelId,
        bool isStreaming
    )
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            LogMissingModel(logger, responseId, requestedModelId, isStreaming);
            return;
        }

        LogSelectedModel(logger, responseId, modelId, requestedModelId, isStreaming);
    }

    [LoggerMessage(
        EventId = 0,
        EventName = nameof(LogSelectedModel),
        Level = LogLevel.Information,
        Message = "AI response {ResponseId} used model {ModelId} for requested model "
            + "{RequestedModelId} (streaming: {IsStreaming})"
    )]
    private static partial void LogSelectedModel(
        ILogger logger,
        string? responseId,
        string modelId,
        string? requestedModelId,
        bool isStreaming
    );

    [LoggerMessage(
        EventId = 1,
        EventName = nameof(LogMissingModel),
        Level = LogLevel.Warning,
        Message = "AI response {ResponseId} did not report a model ID for requested model "
            + "{RequestedModelId} (streaming: {IsStreaming}); the selected model cannot be determined"
    )]
    private static partial void LogMissingModel(
        ILogger logger,
        string? responseId,
        string? requestedModelId,
        bool isStreaming
    );
}

public static class ModelLoggingChatClientExtensions
{
    extension(ChatClientBuilder builder)
    {
        /// <summary>
        ///     Logs the model reported by each provider response without capturing message contents.
        /// </summary>
        /// <returns>The configured <see cref="ChatClientBuilder" /> for fluent chaining.</returns>
        public ChatClientBuilder UseModelLogging()
        {
            return builder.Use(
                (innerClient, services) =>
                    new ModelLoggingChatClient(
                        innerClient,
                        services.GetRequiredService<ILogger<ModelLoggingChatClient>>()
                    )
            );
        }
    }
}
