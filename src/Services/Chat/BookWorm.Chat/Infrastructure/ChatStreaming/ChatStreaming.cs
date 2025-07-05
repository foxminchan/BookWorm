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
