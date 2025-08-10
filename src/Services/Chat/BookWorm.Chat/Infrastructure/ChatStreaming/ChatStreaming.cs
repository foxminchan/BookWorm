using BookWorm.Chat.Extensions;
using BookWorm.Chat.Features;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.Chat.Infrastructure.Backplane;
using BookWorm.Chat.Infrastructure.ChatHistory;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatStreaming(
    IChatHistoryService chatHistoryService,
    IAgentOrchestrationService orchestrationService,
    AppSettings appSettings,
    ILogger<ChatStreaming> logger,
    RedisBackplaneService backplaneService
) : IChatStreaming
{
    private readonly TimeSpan _defaultStreamItemTimeout = appSettings.StreamTimeout;

    public async Task AddStreamingMessage(Guid conversationId, string text)
    {
        logger.LogInformation(
            "Adding streaming message for conversation {ConversationId}",
            conversationId
        );

        chatHistoryService.AddUserMessage(text);

        var messages = await chatHistoryService.FetchPromptMessagesAsync(conversationId, text);
        var history = messages.ToChatHistory();
        chatHistoryService.AddMessages(history);

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
            var lastUserMessage = chatHistoryService.GetLastUserMessage();

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

            // Process agents using orchestration service
            var combinedMessage = await orchestrationService.ProcessAgentsSequentiallyAsync(
                lastUserMessage,
                conversationId,
                assistantReplyId,
                tokenSource.Token
            );

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
            chatHistoryService.AddAssistantMessage(combinedMessage);

            // Save assistant's message to database
            await chatHistoryService.SaveAssistantMessageAsync(
                conversationId,
                assistantReplyId,
                combinedMessage
            );
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
}
