using BookWorm.Chat.Domain.AggregatesModel;

namespace BookWorm.Chat.Features;

public static class DomainToDtoMapper
{
    public static ConversationDto ToConversationDto(this Conversation conversation)
    {
        return new(
            conversation.Id,
            conversation.Name,
            conversation.UserId,
            [.. conversation.Messages.ToConversationMessagesDtos()]
        );
    }

    public static IReadOnlyList<ConversationDto> ToConversationDtos(
        this IEnumerable<Conversation> conversations
    )
    {
        return [.. conversations.Select(c => c.ToConversationDto())];
    }

    private static ConversationMessageDto ToConversationMessageDto(this ConversationMessage message)
    {
        return new(
            message.Id,
            message.Text,
            message.Role,
            message.ParentMessageId,
            message.CreatedAt
        );
    }

    private static IReadOnlyList<ConversationMessageDto> ToConversationMessagesDtos(
        this IEnumerable<ConversationMessage> messages
    )
    {
        return [.. messages.Select(m => m.ToConversationMessageDto())];
    }
}
