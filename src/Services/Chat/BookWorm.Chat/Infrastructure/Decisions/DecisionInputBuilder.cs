using System.Text;
using System.Text.Json;
using BookWorm.Chat.Orchestration;

namespace BookWorm.Chat.Infrastructure.Decisions;

internal static class DecisionInputBuilder
{
    private const int MaxPriorContextMessages = 4;

    internal static ChatMessage GetLatestUserMessage(IReadOnlyList<ChatMessage> messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i].Role == ChatRole.User)
            {
                return messages[i];
            }
        }

        return messages[^1];
    }

    internal static string Build(IReadOnlyList<ChatMessage> messages, ChatMessage latestUserMessage)
    {
        var priorMessages = CollectPriorMessages(messages, latestUserMessage);
        var hasClientContext = HasStorefrontContext();

        if (priorMessages.Count == 0 && !hasClientContext)
        {
            return latestUserMessage.Text;
        }

        var sb = new StringBuilder();
        sb.AppendLine(latestUserMessage.Text);

        if (priorMessages.Count > 0)
        {
            AppendPriorMessages(sb, priorMessages);
        }

        if (hasClientContext)
        {
            AppendStorefrontContext(sb);
        }

        return sb.ToString();
    }

    private static List<ChatMessage> CollectPriorMessages(
        IReadOnlyList<ChatMessage> messages,
        ChatMessage latestUserMessage
    )
    {
        List<ChatMessage> priorMessages = [];
        for (
            var i = messages.Count - 1;
            i >= 0 && priorMessages.Count < MaxPriorContextMessages;
            i--
        )
        {
            var candidate = messages[i];
            if (IsRelevantPriorMessage(candidate, latestUserMessage))
            {
                priorMessages.Add(candidate);
            }
        }

        priorMessages.Reverse();
        return priorMessages;
    }

    private static bool IsRelevantPriorMessage(ChatMessage candidate, ChatMessage latestUserMessage)
    {
        return !ReferenceEquals(candidate, latestUserMessage)
            && candidate.Role != ChatRole.System
            && !string.IsNullOrWhiteSpace(candidate.Text);
    }

    private static void AppendPriorMessages(StringBuilder sb, List<ChatMessage> priorMessages)
    {
        sb.AppendLine();
        sb.AppendLine("Recent conversation context:");
        foreach (var prior in priorMessages)
        {
            sb.Append(prior.Role.Value).Append(": ").AppendLine(prior.Text);
        }
    }

    private static bool HasJsonValue(JsonElement? element)
    {
        return element is { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null };
    }

    private static bool HasStorefrontContext()
    {
        var aguiState = AGUIRunContext.Current;
        return aguiState is not null
            && (
                HasJsonValue(aguiState.Context)
                || HasJsonValue(aguiState.State)
                || aguiState.ClientTools.Count > 0
            );
    }

    private static void AppendStorefrontContext(StringBuilder sb)
    {
        var aguiState = AGUIRunContext.Current;
        if (aguiState is null)
        {
            return;
        }

        sb.AppendLine();
        sb.AppendLine("Storefront client context:");

        if (HasJsonValue(aguiState.Context))
        {
            sb.Append("context: ");
            sb.AppendLine(aguiState.Context!.Value.GetRawText());
        }

        if (HasJsonValue(aguiState.State))
        {
            sb.Append("state: ");
            sb.AppendLine(aguiState.State!.Value.GetRawText());
        }

        if (aguiState.ClientTools.Count <= 0)
        {
            return;
        }

        sb.Append("client_tools: ");
        sb.AppendLine(string.Join(", ", aguiState.ClientTools.Select(static t => t.Name)));
    }
}
