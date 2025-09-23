using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Features;
using BookWorm.Chat.Infrastructure.Backplane;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Infrastructure.ChatHistory;

public sealed class ChatHistoryService(
    McpClient mcpClient,
    IServiceScopeFactory scopeFactory,
    RedisBackplaneService backplaneService
) : IChatHistoryService
{
    private readonly Microsoft.SemanticKernel.ChatCompletion.ChatHistory _messages = [];

    public void AddUserMessage(string text)
    {
        _messages.AddUserMessage(text);
    }

    public void AddAssistantMessage(string text)
    {
        _messages.AddAssistantMessage(text);
    }

    public void AddMessages(Microsoft.SemanticKernel.ChatCompletion.ChatHistory history)
    {
        _messages.AddRange(history);
    }

    public string? GetLastUserMessage()
    {
        return _messages.LastOrDefault(m => m.Role == AuthorRole.User)?.Content;
    }

    public Microsoft.SemanticKernel.ChatCompletion.ChatHistory GetChatHistory()
    {
        return _messages;
    }

    public async Task<List<ChatMessage>> SaveUserMessageAndGetHistoryAsync(
        Guid conversationId,
        string text
    )
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        var conversation = await repository.GetByIdAsync(conversationId);
        Guard.Against.NotFound(conversation, conversationId);

        var parentMessage = conversation.Messages.MaxBy(m => m.CreatedAt);

        var message = new ConversationMessage(null, text, AuthorRole.User.Label, parentMessage?.Id);

        conversation.AddMessage(message);

        await repository.AddAsync(conversation);

        var messages = conversation
            .Messages.Select(m => new ChatMessage(new(m.Role!), m.Text))
            .ToList();

        var fragment = new ClientMessageFragment(
            message.Id,
            AuthorRole.User.Label,
            text,
            Guid.CreateVersion7(),
            true
        );

        await backplaneService.ConversationState.PublishFragmentAsync(conversationId, fragment);

        return messages;
    }

    public async Task SaveAssistantMessageAsync(Guid conversationId, Guid messageId, string text)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        var conversation = await repository.GetByIdAsync(conversationId);

        if (conversation is not null)
        {
            var parentMessage = conversation
                .Messages.Where(m => m.Role == AuthorRole.User.Label)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            var message = new ConversationMessage(
                messageId,
                text,
                AuthorRole.Assistant.Label,
                parentMessage?.Id
            );

            conversation.AddMessage(message);

            await repository.UnitOfWork.SaveChangesAsync();
        }

        await backplaneService.ConversationState.CompleteAsync(conversationId, messageId);
    }

    public async Task<List<ChatMessage>> FetchPromptMessagesAsync(Guid conversationId, string text)
    {
        var prompts = await mcpClient.MapToChatMessagesAsync();

        var messages = await SaveUserMessageAndGetHistoryAsync(conversationId, text);

        prompts.AddRange(messages);

        return prompts;
    }
}
