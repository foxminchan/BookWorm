using BookWorm.Chat.Infrastructure.Backplane.Contracts;

namespace BookWorm.Chat.Infrastructure.Backplane;

public sealed class RedisBackplaneService(
    IConversationState conversationState,
    ICancellationManager cancellationManager
)
{
    public IConversationState ConversationState { get; } = conversationState;
    public ICancellationManager CancellationManager { get; } = cancellationManager;
}
