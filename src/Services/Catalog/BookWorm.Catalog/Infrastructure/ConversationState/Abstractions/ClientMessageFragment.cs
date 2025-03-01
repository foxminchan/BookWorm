namespace BookWorm.Catalog.Infrastructure.ConversationState.Abstractions;

public sealed record ClientMessageFragment(
    Guid Id,
    string Sender,
    string Text,
    Guid FragmentId,
    bool IsFinal = false
);
