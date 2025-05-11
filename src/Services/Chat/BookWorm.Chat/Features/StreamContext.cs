namespace BookWorm.Chat.Features;

public sealed record StreamContext(Guid? LastMessageId, Guid? LastFragmentId);
