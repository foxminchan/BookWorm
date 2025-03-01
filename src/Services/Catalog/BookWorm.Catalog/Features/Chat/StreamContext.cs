namespace BookWorm.Catalog.Features.Chat;

public sealed record StreamContext(Guid? LastMessageId, Guid? LastFragmentId);
