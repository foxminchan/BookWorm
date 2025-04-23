using BookWorm.Catalog.Infrastructure.GenAi.ChatStreaming;
using BookWorm.Catalog.Infrastructure.GenAi.ConversationState.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace BookWorm.Catalog.Features.Chat.Stream;

public sealed class ChatStreamHub : Hub
{
    public IAsyncEnumerable<ClientMessageFragment> Stream(
        Guid id,
        StreamContext streamContext,
        IChatStreaming streaming,
        CancellationToken token
    )
    {
        return StreamAsync();

        async IAsyncEnumerable<ClientMessageFragment> StreamAsync()
        {
            await foreach (
                var message in streaming.GetMessageStream(
                    id,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    token
                )
            )
            {
                yield return message;
            }
        }
    }
}
