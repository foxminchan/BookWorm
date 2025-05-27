namespace BookWorm.Chat.Features;

public sealed record ConversationDto(
    Guid Id,
    string? Name,
    Guid? UserId,
    IReadOnlyList<ConversationMessageDto> Messages
);
