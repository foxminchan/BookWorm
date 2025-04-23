namespace BookWorm.Catalog.Infrastructure.GenAi.ConversationState.Abstractions;

public sealed record ClientMessage(Guid Id, string Sender, string Text);
