namespace BookWorm.Chat.Infrastructure.Backplane.Contracts;

public sealed record ClientMessage(Guid Id, string Sender, string Text);
