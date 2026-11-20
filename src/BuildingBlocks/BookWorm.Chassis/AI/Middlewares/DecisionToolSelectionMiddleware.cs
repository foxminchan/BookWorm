using System.Runtime.CompilerServices;
using System.Text;
using BookWorm.Chassis.AI.Decisions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Middlewares;

public sealed class DecisionToolSelectionOptions
{
    public double ProbabilityThreshold { get; set; } = 0.5;

    public int MaxContextMessages { get; set; } = 6;

    public IReadOnlySet<string> AlwaysIncludedToolNames { get; set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    internal void Validate()
    {
        if (double.IsNaN(ProbabilityThreshold) || ProbabilityThreshold is < 0.0 or > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ProbabilityThreshold),
                ProbabilityThreshold,
                "ProbabilityThreshold must be between 0.0 and 1.0."
            );
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(MaxContextMessages, 1);
    }
}

internal static partial class DecisionToolSelectionMiddleware
{
    public static ChatClientBuilder UseDecisionToolSelection(
        ChatClientBuilder builder,
        IDecisionClient decisionClient,
        DecisionToolSelectionOptions? options = null,
        ILogger? logger = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(decisionClient);

        var resolvedOptions = options ?? new DecisionToolSelectionOptions();
        resolvedOptions.Validate();

        return builder.Use(innerClient => new DecisionToolSelectionChatClient(
            innerClient,
            decisionClient,
            resolvedOptions,
            logger
        ));
    }

    private sealed partial class DecisionToolSelectionChatClient(
        IChatClient innerClient,
        IDecisionClient decisionClient,
        DecisionToolSelectionOptions selectionOptions,
        ILogger? logger
    ) : DelegatingChatClient(innerClient)
    {
        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var messageList = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
            var filteredOptions = await SelectToolsAsync(messageList, options, cancellationToken)
                .ConfigureAwait(false);

            return await base.GetResponseAsync(messageList, filteredOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            var messageList = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
            var filteredOptions = await SelectToolsAsync(messageList, options, cancellationToken)
                .ConfigureAwait(false);

            await foreach (
                var update in base.GetStreamingResponseAsync(
                        messageList,
                        filteredOptions,
                        cancellationToken
                    )
                    .ConfigureAwait(false)
            )
            {
                yield return update;
            }
        }

        private async ValueTask<ChatOptions?> SelectToolsAsync(
            IReadOnlyList<ChatMessage> messages,
            ChatOptions? options,
            CancellationToken cancellationToken
        )
        {
            if (options?.Tools is not { Count: > 0 } || options.ToolMode is NoneChatToolMode)
            {
                return options;
            }

            var state = BuildDecisionState(messages, selectionOptions.MaxContextMessages);
            if (string.IsNullOrWhiteSpace(state))
            {
                return options;
            }

            var requiredToolName = (options.ToolMode as RequiredChatToolMode)?.RequiredFunctionName;
            HashSet<AITool> alwaysIncluded = [];
            List<(AITool Tool, string QuestionId)> candidates = [];
            List<BinaryDecisionQuestion> questions = [];
            HashSet<string> seenQuestionIds = [with(StringComparer.Ordinal)];

            for (var i = 0; i < options.Tools.Count; i++)
            {
                var tool = options.Tools[i];
                if (
                    selectionOptions.AlwaysIncludedToolNames.Contains(tool.Name)
                    || (
                        requiredToolName is not null
                        && string.Equals(
                            tool.Name,
                            requiredToolName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                {
                    alwaysIncluded.Add(tool);
                    continue;
                }

                var questionId = seenQuestionIds.Add(tool.Name) ? tool.Name : $"{tool.Name}_{i}";
                seenQuestionIds.Add(questionId);

                var instructions = string.IsNullOrWhiteSpace(tool.Description)
                    ? $"Should the '{tool.Name}' tool be available to handle this request?"
                    : $"Should the '{tool.Name}' tool be available to handle this request? Tool description: {tool.Description}";

                candidates.Add((tool, questionId));
                questions.Add(
                    new BinaryDecisionQuestion(
                        questionId,
                        instructions,
                        "Yes, this tool is relevant and may be needed to answer or act on the request.",
                        "No, this tool is not relevant to the request."
                    )
                );
            }

            if (questions.Count == 0)
            {
                return options;
            }

            DecisionResponse response;
            try
            {
                response = await decisionClient
                    .GetDecisionAsync(state, questions, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
                when (ex is not OperationCanceledException
                    || !cancellationToken.IsCancellationRequested
                )
            {
                if (logger is not null)
                {
                    LogSelectionFailed(logger, ex, options.Tools.Count);
                }

                return options;
            }

            HashSet<AITool> selectedByDecision = [];
            foreach (var (tool, questionId) in candidates)
            {
                var answer = response.GetBinaryAnswer(questionId);
                if (
                    answer is null
                    || answer.TrueProbability >= selectionOptions.ProbabilityThreshold
                )
                {
                    selectedByDecision.Add(tool);
                }
            }

            List<AITool> selectedTools =
            [
                .. options.Tools.Where(t =>
                    alwaysIncluded.Contains(t) || selectedByDecision.Contains(t)
                ),
            ];

            if (logger is not null)
            {
                LogToolsSelected(
                    logger,
                    selectedTools.Count,
                    options.Tools.Count,
                    selectionOptions.ProbabilityThreshold
                );
            }

            var filteredOptions = options.Clone();
            filteredOptions.Tools = selectedTools;
            return filteredOptions;
        }

        private static string BuildDecisionState(
            IReadOnlyList<ChatMessage> messages,
            int maxContextMessages
        )
        {
            List<ChatMessage> relevant = [with(Math.Min(messages.Count, maxContextMessages))];
            for (var i = messages.Count - 1; i >= 0 && relevant.Count < maxContextMessages; i--)
            {
                var message = messages[i];
                if (message.Role == ChatRole.System || string.IsNullOrWhiteSpace(message.Text))
                {
                    continue;
                }

                relevant.Add(message);
            }

            if (relevant.Count == 0)
            {
                return string.Empty;
            }

            relevant.Reverse();

            var sb = new StringBuilder();
            foreach (var message in relevant)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(message.Role.Value).Append(": ").Append(message.Text);
            }

            return sb.ToString();
        }

        [LoggerMessage(
            EventId = 0,
            EventName = nameof(LogSelectionFailed),
            Level = LogLevel.Warning,
            Message = "Decision tool selection failed; falling back to all {ToolCount} tools."
        )]
        private static partial void LogSelectionFailed(
            ILogger logger,
            Exception exception,
            int toolCount
        );

        [LoggerMessage(
            EventId = 1,
            EventName = nameof(LogToolsSelected),
            Level = LogLevel.Debug,
            Message = "Decision tool selection retained {SelectedCount}/{TotalCount} tools (threshold={Threshold:F2})."
        )]
        private static partial void LogToolsSelected(
            ILogger logger,
            int selectedCount,
            int totalCount,
            double threshold
        );
    }
}

public static class DecisionToolSelectionMiddlewareExtensions
{
    extension(ChatClientBuilder builder)
    {
        /// <summary>
        ///     Adds decision-based tool selection middleware to the chat client pipeline,
        ///     using <see cref="IDecisionClient" /> to filter <see cref="ChatOptions.Tools" /> per request.
        /// </summary>
        /// <param name="sp">The DI service provider used to resolve <see cref="IDecisionClient" />.</param>
        /// <param name="options">Optional configuration for tool selection thresholds and context.</param>
        /// <returns>The <see cref="ChatClientBuilder" /> so that additional calls can be chained.</returns>
        public ChatClientBuilder UseDecisionToolSelection(
            IServiceProvider sp,
            DecisionToolSelectionOptions? options = null
        )
        {
            ArgumentNullException.ThrowIfNull(sp);

            var decisionClient = sp.GetRequiredService<IDecisionClient>();
            var logger = sp.GetService<ILoggerFactory>()
                ?.CreateLogger("BookWorm.Chassis.AI.Middlewares.DecisionToolSelection");

            return DecisionToolSelectionMiddleware.UseDecisionToolSelection(
                builder,
                decisionClient,
                options,
                logger
            );
        }

        /// <summary>
        ///     Adds decision-based tool selection middleware to the chat client pipeline
        ///     using the specified <see cref="IDecisionClient" />.
        /// </summary>
        /// <param name="decisionClient">The decision client used to evaluate tool relevance.</param>
        /// <param name="options">Optional configuration for tool selection thresholds and context.</param>
        /// <param name="logger">Optional logger for diagnostics.</param>
        /// <returns>The <see cref="ChatClientBuilder" /> so that additional calls can be chained.</returns>
        public ChatClientBuilder UseDecisionToolSelection(
            IDecisionClient decisionClient,
            DecisionToolSelectionOptions? options = null,
            ILogger? logger = null
        )
        {
            return DecisionToolSelectionMiddleware.UseDecisionToolSelection(
                builder,
                decisionClient,
                options,
                logger
            );
        }
    }
}
