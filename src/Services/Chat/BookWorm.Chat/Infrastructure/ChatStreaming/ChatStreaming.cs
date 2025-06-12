using BookWorm.Chat.Domain.AggregatesModel;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatStreaming : IChatStreaming
{
    private readonly ICancellationManager _cancellationManager;
    private readonly IChatClient _chatClient;
    private readonly IConversationState _conversationState;

    private readonly TimeSpan _defaultStreamItemTimeout;
    private readonly ILogger<ChatStreaming> _logger;
    private readonly IMcpClient _mcpClient;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatStreaming(
        IChatClient chatClient,
        ILogger<ChatStreaming> logger,
        IConversationState conversationState,
        ICancellationManager cancellationManager,
        AppSettings appSettings,
        IMcpClient mcpClient,
        IServiceScopeFactory scopeFactory
    )
    {
        _chatClient = chatClient;
        _logger = logger;
        _conversationState = conversationState;
        _cancellationManager = cancellationManager;
        _mcpClient = mcpClient;
        _scopeFactory = scopeFactory;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "ChatModel: {model}",
                chatClient.GetService<ChatClientMetadata>()?.DefaultModelId
            );
        }

        _defaultStreamItemTimeout = appSettings.StreamTimeout;

        Messages = [];
    }

    private List<ChatMessage> Messages { get; }

    public async Task AddStreamingMessage(Guid conversationId, string text)
    {
        await FetchAndAddPromptMessages(conversationId, text);

        var tools = await _mcpClient.ListToolsAsync();

        var chatOptions = new ChatOptions { Tools = [.. tools] };

        _ = Task.Run(() => StreamReplyAsync(conversationId, chatOptions));
    }

    private async Task StreamReplyAsync(Guid conversationId, ChatOptions chatOptions)
    {
        var assistantReplyId = Guid.CreateVersion7();

        _logger.LogInformation(
            "Adding streaming message for conversation {ConversationId} {MessageId}",
            conversationId,
            assistantReplyId
        );

        var allChunks = new List<ChatResponseUpdate>();

        var token = _cancellationManager.GetCancellationToken(assistantReplyId);

        var fragment = new ClientMessageFragment(
            assistantReplyId,
            ChatRole.Assistant.Value,
            "Generating reply...",
            Guid.CreateVersion7()
        );
        await _conversationState.PublishFragmentAsync(conversationId, fragment);

        try
        {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            tokenSource.CancelAfter(_defaultStreamItemTimeout);

            await foreach (
                var update in _chatClient
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
                await _conversationState.PublishFragmentAsync(conversationId, fragment);
            }

            _logger.LogInformation(
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
            _logger.LogError(
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
        }
        catch (Exception ex)
        {
            fragment = new(
                assistantReplyId,
                ChatRole.Assistant.Value,
                "Error streaming message",
                Guid.CreateVersion7()
            );
            await _conversationState.PublishFragmentAsync(conversationId, fragment);
            _logger.LogError(
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
            await _conversationState.PublishFragmentAsync(conversationId, fragment);
            await _cancellationManager.CancelAsync(assistantReplyId);
        }
    }

    public async IAsyncEnumerable<ClientMessageFragment> GetMessageStream(
        Guid conversationId,
        Guid? lastMessageId,
        Guid? lastDeliveredFragment,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Getting message stream for conversation {ConversationId}, {LastMessageId}",
            conversationId,
            lastMessageId
        );
        var stream = _conversationState.Subscribe(conversationId, lastMessageId, cancellationToken);

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

    private async Task FetchAndAddPromptMessages(Guid conversationId, string text)
    {
        var prompts = await _mcpClient.ListPromptsAsync();

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
        await using var scope = _scopeFactory.CreateAsyncScope();

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

        await _conversationState.PublishFragmentAsync(id, fragment);

        return messages;
    }

    private async Task SaveAssistantMessageToDatabase(
        Guid conversationId,
        Guid messageId,
        string text
    )
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

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

        await _conversationState.CompleteAsync(conversationId, messageId);
    }
}
