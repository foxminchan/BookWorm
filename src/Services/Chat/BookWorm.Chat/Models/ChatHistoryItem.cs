using Microsoft.Extensions.VectorData;

namespace BookWorm.Chat.Models;

public sealed class ChatHistoryItem
{
    [VectorStoreKey]
    public string? Key { get; set; }

    [VectorStoreData]
    public string? ThreadId { get; set; }

    [VectorStoreData]
    public DateTimeOffset? Timestamp { get; set; }

    [VectorStoreData]
    public string? SerializedMessage { get; set; }

    [VectorStoreData]
    public string? MessageText { get; set; }

    [VectorStoreData]
    public string? UserId { get; set; }
}
