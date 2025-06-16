namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatContext(
    IConversationState conversationState,
    ICancellationManager cancellationManager
)
{
    public IConversationState ConversationState { get; } = conversationState;
    public ICancellationManager CancellationManager { get; } = cancellationManager;
}
