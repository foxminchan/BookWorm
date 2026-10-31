using BookWorm.Chat.Memory.Extraction;
using Microsoft.Agents.AI;

namespace BookWorm.Chat.Memory;

/// <summary>
///     Gives BookAgent durable, user-scoped episodic and semantic memory across sessions.
/// </summary>
/// <remarks>
///     <para>
///         Before a run, the provider resolves the authenticated user for that invocation, recalls their
///         strongest preferences and the past episodes relevant to the latest message, and injects them as
///         transient instructions. When no user is authenticated or nothing is remembered, nothing is injected.
///     </para>
///     <para>
///         After a successful run, the exchange is queued for background extraction without blocking the
///         response stream. The provider keeps no per-user state, so it is safe for concurrent invocations.
///     </para>
/// </remarks>
internal sealed class UserMemoryContextProvider(
    IUserIdentityAccessor identityAccessor,
    IUserMemoryService memoryService,
    IMemoryExtractionQueue extractionQueue,
    ILogger<UserMemoryContextProvider> logger
) : AIContextProvider
{
    // Bound the text queued for extraction; long responses add cost without adding memorable signal.
    private const int MaxQueuedTextLength = 4_000;

    protected override async ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var userId = identityAccessor.GetUserId();

        if (userId is null)
        {
            return new();
        }

        var query = GetLastText(context.AIContext.Messages, ChatRole.User);

        var snapshot = await memoryService.RecallAsync(userId, query, cancellationToken);

        var instructions = MemoryPromptFormatter.Format(snapshot);

        if (instructions is null)
        {
            return new();
        }

        logger.LogDebug(
            "Injected {PreferenceCount} preferences and {EpisodeCount} episodes into BookAgent context",
            snapshot.Preferences.Count,
            snapshot.Episodes.Count
        );

        return new() { Instructions = instructions };
    }

    protected override ValueTask StoreAIContextAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default
    )
    {
        var userId = identityAccessor.GetUserId();

        if (userId is null)
        {
            return ValueTask.CompletedTask;
        }

        var userMessage = GetLastText(context.RequestMessages, ChatRole.User);

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return ValueTask.CompletedTask;
        }

        var assistantResponse = string.Join(
            Environment.NewLine,
            (context.ResponseMessages ?? [])
                .Where(m => m.Role == ChatRole.Assistant && !string.IsNullOrWhiteSpace(m.Text))
                .Select(m => m.Text)
        );

        var accepted = extractionQueue.TryEnqueue(
            new(
                userId,
                Truncate(userMessage, MaxQueuedTextLength),
                Truncate(assistantResponse, MaxQueuedTextLength)
            )
        );

        if (!accepted)
        {
            logger.LogDebug("Memory extraction queue rejected a BookAgent exchange");
        }

        return ValueTask.CompletedTask;
    }

    private static string? GetLastText(IEnumerable<ChatMessage>? messages, ChatRole role)
    {
        return messages
            ?.LastOrDefault(m => m.Role == role && !string.IsNullOrWhiteSpace(m.Text))
            ?.Text;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
