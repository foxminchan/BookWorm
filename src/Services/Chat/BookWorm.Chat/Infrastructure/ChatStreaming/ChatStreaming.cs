using BookWorm.Chat.Domain.AggregatesModel;
using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatStreaming(
    IChatClient chatClient,
    ILogger<ChatStreaming> logger,
    ChatContext chatContext,
    AppSettings appSettings,
    IMcpClient mcpClient,
    GlobalLogBuffer logBuffer,
    IServiceScopeFactory scopeFactory
) : IChatStreaming
{
    private readonly TimeSpan _defaultStreamItemTimeout = appSettings.StreamTimeout;
    private List<ChatMessage> Messages { get; } = [];

    public async Task AddStreamingMessage(Guid conversationId, string text)
    {
        await FetchAndAddPromptMessages(conversationId, text);

        var tools = await mcpClient.ListToolsAsync();

        var chatOptions = new ChatOptions { Tools = [.. tools] };

        _ = Task.Run(() => StreamReplyAsync(conversationId, chatOptions));
    }

    public async IAsyncEnumerable<ClientMessageFragment> GetMessageStream(
        Guid conversationId,
        Guid? lastMessageId,
        Guid? lastDeliveredFragment,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Getting message stream for conversation {ConversationId}, {LastMessageId}",
            conversationId,
            lastMessageId
        );
        var stream = chatContext.ConversationState.Subscribe(
            conversationId,
            lastMessageId,
            cancellationToken
        );

        await foreach (var fragment in stream)
        {
            if (lastDeliveredFragment is null || fragment.FragmentId > lastDeliveredFragment)
            {
                lastDeliveredFragment = fragment.FragmentId;
            }
            else
            {
                continue;
            }

            yield return fragment;
        }
    }

    private async Task StreamReplyAsync(Guid conversationId, ChatOptions chatOptions)
    {
        var assistantReplyId = Guid.CreateVersion7();

        logger.LogInformation(
            "Adding streaming message for conversation {ConversationId} {MessageId}",
            conversationId,
            assistantReplyId
        );

        var allChunks = new List<ChatResponseUpdate>();

        var token = chatContext.CancellationManager.GetCancellationToken(assistantReplyId);

        var fragment = new ClientMessageFragment(
            assistantReplyId,
            ChatRole.Assistant.Value,
            "Generating reply...",
            Guid.CreateVersion7()
        );
        await chatContext.ConversationState.PublishFragmentAsync(conversationId, fragment);

        try
        {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            tokenSource.CancelAfter(_defaultStreamItemTimeout);

            await foreach (
                var update in chatClient
                    .GetStreamingResponseAsync(Messages, chatOptions)
                    .WithCancellation(tokenSource.Token)
            )
            {
                tokenSource.CancelAfter(_defaultStreamItemTimeout);

                allChunks.Add(update);
                fragment = new(
                    assistantReplyId,
                    ChatRole.Assistant.Value,
                    update.Text,
                    Guid.CreateVersion7()
                );
                await chatContext.ConversationState.PublishFragmentAsync(conversationId, fragment);
            }

            logger.LogInformation(
                "Full message received for conversation {ConversationId} {MessageId}",
                conversationId,
                assistantReplyId
            );

            if (allChunks.Count > 0)
            {
                var fullMessage = allChunks.ToChatResponse().Text;
                await SaveAssistantMessageToDatabase(conversationId, assistantReplyId, fullMessage);
            }
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(
                ex,
                "Streaming message cancelled for conversation {ConversationId} {MessageId}",
                conversationId,
                assistantReplyId
            );

            if (allChunks.Count > 0)
            {
                var fullMessage = allChunks.ToChatResponse().Text;
                await SaveAssistantMessageToDatabase(conversationId, assistantReplyId, fullMessage);
            }

            logBuffer.Flush();
        }
        catch (Exception ex)
        {
            fragment = new(
                assistantReplyId,
                ChatRole.Assistant.Value,
                "Error streaming message",
                Guid.CreateVersion7()
            );
            await chatContext.ConversationState.PublishFragmentAsync(conversationId, fragment);
            logger.LogError(
                ex,
                "Error streaming message for conversation {ConversationId} {MessageId}",
                conversationId,
                assistantReplyId
            );

            await SaveAssistantMessageToDatabase(
                conversationId,
                assistantReplyId,
                "Error streaming message"
            );

            logBuffer.Flush();
        }
        finally
        {
            fragment = new(
                assistantReplyId,
                ChatRole.Assistant.Value,
                string.Empty,
                Guid.CreateVersion7(),
                true
            );
            await chatContext.ConversationState.PublishFragmentAsync(conversationId, fragment);
            await chatContext.CancellationManager.CancelAsync(assistantReplyId);
        }
    }

    private async Task FetchAndAddPromptMessages(Guid conversationId, string text)
    {
        var prompts = await mcpClient.ListPromptsAsync();

        var promptMessages = await prompts
            .ToAsyncEnumerable()
            .Select(async prompt => (await prompt.GetAsync()).ToChatMessages())
            .SelectMany(x => x.Result)
            .ToListAsync();

        Messages.AddRange(promptMessages);

        var messages = await SavePromptAndGetMessageHistoryAsync(conversationId, text);

        Messages.AddRange(messages);
    }

    private async Task<IList<ChatMessage>> SavePromptAndGetMessageHistoryAsync(Guid id, string text)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        var conversation = await repository.GetByIdAsync(id);

        Guard.Against.NotFound(conversation, id);

        var parentMessage = conversation
            .Messages.OrderByDescending(m => m.CreatedAt)
            .FirstOrDefault();

        var message = new ConversationMessage(null, text, ChatRole.User.Value, parentMessage?.Id);

        conversation.AddMessage(message);

        await repository.AddAsync(conversation);

        var messages = conversation
            .Messages.Select(m => new ChatMessage(new(m.Role!), m.Text))
            .ToList();

        var fragment = new ClientMessageFragment(
            message.Id,
            ChatRole.User.Value,
            text,
            Guid.CreateVersion7(),
            true
        );

        await chatContext.ConversationState.PublishFragmentAsync(id, fragment);

        return messages;
    }

    private async Task SaveAssistantMessageToDatabase(
        Guid conversationId,
        Guid messageId,
        string text
    )
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        var conversation = await repository.GetByIdAsync(conversationId);

        if (conversation is not null)
        {
            var parentMessage = conversation
                .Messages.Where(m => m.Role == ChatRole.User.Value)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            var message = new ConversationMessage(
                messageId,
                text,
                ChatRole.Assistant.Value,
                parentMessage?.Id
            );

            conversation.AddMessage(message);

            await repository.UnitOfWork.SaveChangesAsync();
        }

        await chatContext.ConversationState.CompleteAsync(conversationId, messageId);
    }
}
