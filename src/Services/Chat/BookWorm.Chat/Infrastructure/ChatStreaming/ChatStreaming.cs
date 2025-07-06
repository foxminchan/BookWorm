using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Extensions;
using BookWorm.Chat.Features;
using BookWorm.Chat.Infrastructure.Backplane;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatStreaming(
    IMcpClient mcpClient,
    ChatAgents chatAgents,
    AppSettings appSettings,
    ILogger<ChatStreaming> logger,
    IServiceScopeFactory scopeFactory,
    RedisBackplaneService backplaneService
) : IChatStreaming
{
    private readonly TimeSpan _defaultStreamItemTimeout = appSettings.StreamTimeout;
    private readonly ChatHistory _messages = [];

    /// <summary>
    /// Adds a user message to the conversation, updates the chat history with prompts and previous messages, and initiates asynchronous streaming of the assistant's reply.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="text">The user's message text to add and stream a reply for.</param>
    public async Task AddStreamingMessage(Guid conversationId, string text)
    {
        logger.LogInformation(
            "Adding streaming message for conversation {ConversationId}",
            conversationId
        );

        _messages.AddUserMessage(text);

        var messages = await FetchAndAddPromptMessagesAsync(conversationId, text);
        var history = messages.ToChatHistory();
        _messages.AddRange(history);

        _ = StreamReplyAsync(conversationId)
            .ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        logger.LogError(
                            t.Exception,
                            "Failed to stream reply for conversation {ConversationId}",
                            conversationId
                        );
                    }
                },
                TaskScheduler.Default
            );
    }

    /// <summary>
    /// Asynchronously streams new message fragments for a conversation, starting after the specified fragment.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation to stream messages from.</param>
    /// <param name="lastMessageId">The ID of the last message received by the client, or null to start from the beginning.</param>
    /// <param name="lastDeliveredFragment">The ID of the last delivered message fragment, or null to include all fragments.</param>
    /// <param name="cancellationToken">A token to cancel the streaming operation.</param>
    /// <returns>An asynchronous stream of <see cref="ClientMessageFragment"/> objects representing new message fragments.</returns>
    public async IAsyncEnumerable<ClientMessageFragment> GetMessageStream(
        Guid conversationId,
        Guid? lastMessageId,
        Guid? lastDeliveredFragment,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (cancellationToken.IsCancellationRequested)
        {
            yield break;
        }

        logger.LogInformation(
            "Getting message stream for conversation {ConversationId}, {LastMessageId}",
            conversationId,
            lastMessageId
        );

        var stream = backplaneService.ConversationState.SubscribeAsync(
            conversationId,
            lastMessageId,
            cancellationToken
        );

        await foreach (var fragment in stream)
        {
            if (lastDeliveredFragment is not null && !(fragment.FragmentId > lastDeliveredFragment))
            {
                continue;
            }

            lastDeliveredFragment = fragment.FragmentId;
            yield return fragment;
        }
    }

    /// <summary>
    /// Streams an assistant's reply for a conversation by orchestrating multiple chat agents, publishing partial and final response fragments to the backplane, and saving the completed message to the database.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation for which to stream the assistant's reply.</param>
    private async Task StreamReplyAsync(Guid conversationId)
    {
        var assistantReplyId = Guid.CreateVersion7();

        logger.LogInformation(
            "Adding streaming message for conversation {ConversationId} {MessageId}",
            conversationId,
            assistantReplyId
        );

        var token = backplaneService.CancellationManager.GetCancellationToken(assistantReplyId);
        var fragment = new ClientMessageFragment(
            assistantReplyId,
            AuthorRole.Assistant.Label,
            "Generating reply...",
            Guid.CreateVersion7()
        );

        try
        {
            await backplaneService.ConversationState.PublishFragmentAsync(conversationId, fragment);

            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            tokenSource.CancelAfter(_defaultStreamItemTimeout);

            // Get the last user message
            var lastUserMessage = _messages.LastOrDefault(m => m.Role == AuthorRole.User)?.Content;

            if (string.IsNullOrEmpty(lastUserMessage))
            {
                logger.LogWarning(
                    "No user message found in chat history for conversation {ConversationId}",
                    conversationId
                );
                var errorFragment = new ClientMessageFragment(
                    assistantReplyId,
                    AuthorRole.Assistant.Label,
                    "Unable to process request: No user message found",
                    Guid.CreateVersion7(),
                    true
                );
                await backplaneService.ConversationState.PublishFragmentAsync(
                    conversationId,
                    errorFragment
                );
                return;
            }

            // Use concurrent orchestration to process responses from both agents in parallel
            var runtime = new InProcessRuntime();
            await runtime.StartAsync(token);

            // Callback to observe agent responses during orchestration
            async ValueTask ResponseCallbackAsync(ChatMessageContent response)
            {
                // Add to existing message history for tracking
                _messages.Add(response);

                logger.LogInformation(
                    "Agent response received from {AuthorName}: {Content}",
                    response.AuthorName ?? "Anonymous User",
                    response.Content
                );

                // Stream individual agent responses as they come in
                var agentFragment = new ClientMessageFragment(
                    assistantReplyId,
                    AuthorRole.Assistant.Label,
                    $"**{response.AuthorName ?? "Agent"}**: {response.Content}\n\n",
                    Guid.CreateVersion7()
                );

                await backplaneService.ConversationState.PublishFragmentAsync(
                    conversationId,
                    agentFragment
                );
            }

            SequentialOrchestration orchestration = new(
                chatAgents.LanguageAgent,
                chatAgents.SummarizeAgent,
                chatAgents.SentimentAgent,
                chatAgents.BookAgent
            )
            {
                ResponseCallback = ResponseCallbackAsync,
            };

            // Get result from sequential orchestration - it will handle streaming internally
            var result = await orchestration.InvokeAsync(
                lastUserMessage,
                runtime,
                tokenSource.Token
            );

            // Get final combined results
            var finalResults = await result.GetValueAsync(
                TimeSpan.FromSeconds(60),
                tokenSource.Token
            );

            var combinedMessage = string.Join("\n\n", finalResults);

            // Publish the final fragment
            var finalFragment = new ClientMessageFragment(
                assistantReplyId,
                AuthorRole.Assistant.Label,
                combinedMessage,
                Guid.CreateVersion7(),
                true
            );

            await backplaneService.ConversationState.PublishFragmentAsync(
                conversationId,
                finalFragment
            );

            // Add the completed message to the history
            _messages.AddAssistantMessage(combinedMessage);

            // Save assistant's message to database
            await SaveAssistantMessageToDatabaseAsync(
                conversationId,
                assistantReplyId,
                combinedMessage
            );

            await runtime.RunUntilIdleAsync();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogInformation(
                ex,
                "Message stream cancelled for conversation {ConversationId} {MessageId}",
                conversationId,
                assistantReplyId
            );

            // Publish cancelled fragment
            var cancelledFragment = new ClientMessageFragment(
                assistantReplyId,
                AuthorRole.Assistant.Label,
                "Message generation cancelled",
                Guid.CreateVersion7(),
                true
            );

            await backplaneService.ConversationState.PublishFragmentAsync(
                conversationId,
                cancelledFragment
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error streaming response for conversation {ConversationId} {MessageId}",
                conversationId,
                assistantReplyId
            );

            var errorFragment = new ClientMessageFragment(
                assistantReplyId,
                AuthorRole.Assistant.Label,
                "An error occurred while generating the response",
                Guid.CreateVersion7(),
                true
            );

            await backplaneService.ConversationState.PublishFragmentAsync(
                conversationId,
                errorFragment
            );
        }
    }

    /// <summary>
    /// Retrieves all prompt messages and the conversation history for the specified conversation, adds the user's message, and returns the combined list of chat messages.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="text">The user's message text to add to the conversation.</param>
    /// <returns>A list of chat messages including prompts and updated conversation history.</returns>
    private async Task<List<ChatMessage>> FetchAndAddPromptMessagesAsync(
        Guid conversationId,
        string text
    )
    {
        var prompts = await mcpClient.ListPromptsAsync();

        List<ChatMessage> promptMessages = [];

        foreach (var prompt in prompts)
        {
            var chatMessages = (await prompt.GetAsync()).ToChatMessages();
            promptMessages.AddRange(chatMessages);
        }

        var messages = await SavePromptAndGetMessageHistoryAsync(conversationId, text);

        promptMessages.AddRange(messages);

        return promptMessages;
    }

    /// <summary>
    /// Saves a new user message to the specified conversation and returns the updated chat message history.
    /// </summary>
    /// <param name="id">The unique identifier of the conversation.</param>
    /// <param name="text">The text content of the user message to add.</param>
    /// <returns>A list of chat messages representing the conversation history after the new message is added.</returns>
    private async Task<List<ChatMessage>> SavePromptAndGetMessageHistoryAsync(Guid id, string text)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        var conversation = await repository.GetByIdAsync(id);
        Guard.Against.NotFound(conversation, id);

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

        await backplaneService.ConversationState.PublishFragmentAsync(id, fragment);

        return messages;
    }

    /// <summary>
    /// Saves an assistant message to the database for the specified conversation and marks the message as complete in the backplane.
    /// </summary>
    /// <param name="conversationId">The unique identifier of the conversation.</param>
    /// <param name="messageId">The unique identifier of the assistant message.</param>
    /// <param name="text">The content of the assistant message.</param>
    private async Task SaveAssistantMessageToDatabaseAsync(
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
}
