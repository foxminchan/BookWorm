using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Extensions;

public static class ChatHistoryExtensions
{
    public static ChatHistory ToChatHistory(this List<ChatMessage> messages)
    {
        return [.. messages.Select(message => message.ToChatMessageContent()).ToList()];
    }

    private static ChatMessageContent ToChatMessageContent(this ChatMessage chatMessage)
    {
        return new() { Role = new(chatMessage.Role.Value), Content = chatMessage.Text };
    }
}
