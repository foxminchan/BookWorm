using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Extensions;

public static class ChatHistoryExtensions
{
    public static ChatHistory ToChatHistory(this List<ChatMessage>? messages)
    {
        ChatHistory chatHistory = [];

        if (messages is null)
        {
            return chatHistory;
        }

        foreach (var message in messages)
        {
            chatHistory.AddMessage(message.Role.ToAuthorRole(), message.Text);
        }

        return chatHistory;
    }
}
