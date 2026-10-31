namespace BookWorm.Chat.Memory.Extraction;

/// <summary>
///     A single completed BookAgent exchange queued for background memory extraction.
/// </summary>
/// <param name="UserId">The authenticated user who owns any memory extracted from the exchange.</param>
/// <param name="UserMessage">The user's message (raw; anonymized before it reaches the extractor model).</param>
/// <param name="AssistantResponse">BookAgent's response.</param>
internal sealed record MemoryExtractionRequest(
    string UserId,
    string UserMessage,
    string AssistantResponse
);
