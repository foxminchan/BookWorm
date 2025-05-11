namespace BookWorm.Chat.Infrastructure.ConversationState.Abstractions;

public sealed record ClientMessage(Guid Id, string Sender, string Text);
