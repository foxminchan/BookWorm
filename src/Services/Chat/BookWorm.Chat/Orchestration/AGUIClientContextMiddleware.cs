using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace BookWorm.Chat.Orchestration;

internal static class AGUIClientContextMiddleware
{
    public static ChatClientBuilder UseAGUIClientContext(this ChatClientBuilder builder)
    {
        return builder.Use(innerClient => new AGUIClientContextChatClient(innerClient));
    }

    private sealed class AGUIClientContextChatClient(IChatClient innerClient)
        : DelegatingChatClient(innerClient)
    {
        public override Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var (enrichedMessages, enrichedOptions) = Enrich(messages, options);
            return base.GetResponseAsync(enrichedMessages, enrichedOptions, cancellationToken);
        }

        public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            var (enrichedMessages, enrichedOptions) = Enrich(messages, options);

            await foreach (
                var update in base.GetStreamingResponseAsync(
                        enrichedMessages,
                        enrichedOptions,
                        cancellationToken
                    )
                    .ConfigureAwait(false)
            )
            {
                yield return update;
            }
        }

        private static (IEnumerable<ChatMessage> Messages, ChatOptions? Options) Enrich(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options
        )
        {
            var aguiState = AGUIRunContext.Current;
            if (aguiState is null)
            {
                return (messages, options);
            }

            var enrichedOptions = EnrichOptions(options, aguiState.ClientTools);
            var contextPrompt = BuildContextPrompt(aguiState);

            if (string.IsNullOrEmpty(contextPrompt))
            {
                return (messages, enrichedOptions);
            }

            List<ChatMessage> enrichedMessages = [new(ChatRole.System, contextPrompt), .. messages];
            return (enrichedMessages, enrichedOptions);
        }

        private static ChatOptions? EnrichOptions(
            ChatOptions? options,
            IReadOnlyList<AITool> clientTools
        )
        {
            if (clientTools.Count == 0)
            {
                return options;
            }

            var cloned = options?.Clone() ?? new ChatOptions();
            List<AITool> merged = cloned.Tools is { Count: > 0 } ? [.. cloned.Tools] : [];
            HashSet<string> existingNames =
            [
                with(StringComparer.OrdinalIgnoreCase),
                .. merged.Select(t => t.Name),
            ];

            merged.AddRange(clientTools.Where(tool => existingNames.Add(tool.Name)));

            cloned.Tools = merged;
            return cloned;
        }

        private static string? BuildContextPrompt(AGUIRunState state)
        {
            var hasState =
                state.State is
                { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null };
            var hasContext =
                state.Context is
                { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null };

            if (!hasState && !hasContext)
            {
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Live storefront client context (read-only, do not expose raw JSON):");

            if (hasContext)
            {
                sb.Append("<ag_ui_context>");
                sb.Append(state.Context!.Value.GetRawText());
                sb.AppendLine("</ag_ui_context>");
            }

            if (hasState)
            {
                sb.Append("<ag_ui_state>");
                sb.Append(state.State!.Value.GetRawText());
                sb.AppendLine("</ag_ui_state>");
            }

            return sb.ToString();
        }
    }
}
