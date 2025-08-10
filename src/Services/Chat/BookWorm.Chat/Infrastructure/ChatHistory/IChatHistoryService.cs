namespace BookWorm.Chat.Infrastructure.ChatHistory;

public interface IChatHistoryService
{
    Task<List<ChatMessage>> SaveUserMessageAndGetHistoryAsync(Guid conversationId, string text);
    Task SaveAssistantMessageAsync(Guid conversationId, Guid messageId, string text);
    Task<List<ChatMessage>> FetchPromptMessagesAsync(Guid conversationId, string text);
    void AddUserMessage(string text);
    void AddAssistantMessage(string text);
    void AddMessages(Microsoft.SemanticKernel.ChatCompletion.ChatHistory history);
    string? GetLastUserMessage();
    Microsoft.SemanticKernel.ChatCompletion.ChatHistory GetChatHistory();
}
