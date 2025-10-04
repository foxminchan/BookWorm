using System.Text.Json.Serialization;
using BookWorm.Chat.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chat.Infrastructure.ChatHistory;

public sealed class VectorChatMessageStore : ChatMessageStore
{
    private readonly AppSettings _appSettings;
    private readonly VectorStoreCollection<Guid, ChatHistoryItem> _collection;
    private readonly ClaimsPrincipal _claimsPrincipal;

    public VectorChatMessageStore(
        AppSettings appSettings,
        VectorStoreCollection<Guid, ChatHistoryItem> collection,
        ClaimsPrincipal claimsPrincipal,
        JsonElement serializedStoreState,
        JsonSerializerOptions? jsonSerializerOptions = null
    )
    {
        _appSettings = appSettings;
        _collection = collection;
        _claimsPrincipal = claimsPrincipal;

        if (serializedStoreState.ValueKind is JsonValueKind.String)
        {
            ThreadDbKey = serializedStoreState.Deserialize<string>(jsonSerializerOptions);
        }
    }

    private string? ThreadDbKey { get; set; }

    public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(
        CancellationToken cancellationToken = new()
    )
    {
        await _collection.EnsureCollectionExistsAsync(cancellationToken);

        var userId = _claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier);

        var records = await _collection
            .GetAsync(
                x => x.ThreadId == ThreadDbKey && x.UserId == userId,
                _appSettings.MaxMessages,
                new() { OrderBy = x => x.Descending(y => y.Timestamp) },
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        var messages = records.ConvertAll(x =>
            JsonSerializer.Deserialize<ChatMessage>(
                x.SerializedMessage!,
                ChatMessageSerializationContext.Default.Options
            )!
        );
        messages.Reverse();
        return messages;
    }

    public override async Task AddMessagesAsync(
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken = new()
    )
    {
        ThreadDbKey ??= Guid.CreateVersion7().ToString("N");
        await _collection.EnsureCollectionExistsAsync(cancellationToken);
        await _collection.UpsertAsync(
            messages.Select(x => new ChatHistoryItem
            {
                Key = $"{ThreadDbKey}{x.MessageId}",
                Timestamp = DateTimeOffset.UtcNow,
                ThreadId = ThreadDbKey,
                SerializedMessage = JsonSerializer.Serialize(
                    x,
                    ChatMessageSerializationContext.Default.ChatMessage
                ),
                MessageText = x.Text,
                UserId = _claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier),
            }),
            cancellationToken
        );
    }

    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.SerializeToElement(ThreadDbKey);
    }
}

[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(List<ChatMessage>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class ChatMessageSerializationContext : JsonSerializerContext;
