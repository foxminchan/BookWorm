using System.Diagnostics.CodeAnalysis;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

[ExcludeFromCodeCoverage]
public sealed class ChatContext(
    IConversationState conversationState,
    ICancellationManager cancellationManager
)
{
    public IConversationState ConversationState { get; } = conversationState;
    public ICancellationManager CancellationManager { get; } = cancellationManager;
}
