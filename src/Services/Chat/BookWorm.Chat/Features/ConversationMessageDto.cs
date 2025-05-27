namespace BookWorm.Chat.Features;

public sealed record ConversationMessageDto(
    Guid Id,
    string? Text,
    string? Role,
    Guid? ParentMessageId,
    DateTime CreatedAt
);
