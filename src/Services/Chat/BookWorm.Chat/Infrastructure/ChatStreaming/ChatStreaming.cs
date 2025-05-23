﻿namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatStreaming : IChatStreaming
{
    private readonly ICancellationManager _cancellationManager;
    private readonly IChatClient _chatClient;
    private readonly IConversationState _conversationState;

    private readonly TimeSpan _defaultStreamItemTimeout;
    private readonly ILogger<ChatStreaming> _logger;
    private readonly IMcpClient _mcpClient;

    public ChatStreaming(
        IChatClient chatClient,
        ILogger<ChatStreaming> logger,
        IConversationState conversationState,
        ICancellationManager cancellationManager,
        AppSettings appSettings,
        IMcpClient mcpClient
    )
    {
        _chatClient = chatClient;
        _logger = logger;
        _conversationState = conversationState;
        _cancellationManager = cancellationManager;
        _mcpClient = mcpClient;

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

    /// <summary>
    ///     Adds a new user message to the conversation and starts streaming the AI response.
    /// </summary>
    /// <param name="text">The text message from the user.</param>
    /// <returns>A unique identifier for the conversation.</returns>
    public async Task<Guid> AddStreamingMessage(string text)
    {
        var conversationId = Guid.CreateVersion7();

        var tools = await _mcpClient.ListToolsAsync();

        var chatOptions = new ChatOptions { Tools = [.. tools] };

        var prompts = await _mcpClient.ListPromptsAsync();

        var promptMessages = await Task.WhenAll(
            prompts.Select(async prompt => (await prompt.GetAsync()).ToChatMessages())
        );

        Messages.AddRange(promptMessages.SelectMany(messages => messages));

        var fragment = new ClientMessageFragment(
            conversationId,
            ChatRole.User.Value,
            text,
            Guid.CreateVersion7(),
            true
        );

        await _conversationState.PublishFragmentAsync(conversationId, fragment);

        Messages.Add(new(ChatRole.User, text));

        _ = Task.Run(StreamReplyAsync);

        return conversationId;

        async Task StreamReplyAsync()
        {
            var assistantReplyId = Guid.CreateVersion7();

            _logger.LogInformation(
                "Adding streaming message for conversation {ConversationId} {MessageId}",
                conversationId,
                assistantReplyId
            );

            var allChunks = new List<ChatResponseUpdate>();

            var token = _cancellationManager.GetCancellationToken(assistantReplyId);

            fragment = new(
                assistantReplyId,
                ChatRole.Assistant.Value,
                "Thinking...",
                Guid.CreateVersion7()
            );

            await _conversationState.PublishFragmentAsync(conversationId, fragment);

            try
            {
                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                tokenSource.CancelAfter(_defaultStreamItemTimeout);

                await foreach (
                    var chatResponseUpdate in _chatClient
                        .GetStreamingResponseAsync(Messages, chatOptions)
                        .WithCancellation(tokenSource.Token)
                )
                {
                    tokenSource.CancelAfter(_defaultStreamItemTimeout);

                    fragment = new(
                        assistantReplyId,
                        ChatRole.Assistant.Value,
                        chatResponseUpdate.Text,
                        Guid.CreateVersion7()
                    );

                    allChunks.Add(chatResponseUpdate);
                    await _conversationState.PublishFragmentAsync(conversationId, fragment);
                }

                _logger.LogInformation(
                    "Full message received for conversation {ConversationId} {MessageId}",
                    conversationId,
                    assistantReplyId
                );

                if (allChunks.Count > 0)
                {
                    await _conversationState.CompleteAsync(conversationId, assistantReplyId);
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
                    await _conversationState.CompleteAsync(conversationId, assistantReplyId);
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

                if (allChunks.Count > 0)
                {
                    await _conversationState.CompleteAsync(conversationId, assistantReplyId);
                }
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
    }

    /// <summary>
    ///     Retrieves a stream of message fragments for a specific conversation.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="lastMessageId">Optional identifier of the last message received.</param>
    /// <param name="lastDeliveredFragment">Optional identifier of the last delivered fragment.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous stream of message fragments.</returns>
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
}
