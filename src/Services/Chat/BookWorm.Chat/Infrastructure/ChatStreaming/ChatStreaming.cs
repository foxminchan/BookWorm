using BookWorm.Chat.Features;
using BookWorm.Chat.Infrastructure.AgentOrchestration;
using BookWorm.Chat.Infrastructure.Backplane;

namespace BookWorm.Chat.Infrastructure.ChatStreaming;

public sealed class ChatStreaming(
    IAgentOrchestrationService orchestrationService,
    RedisBackplaneService backplaneService,
    ILogger<ChatStreaming> logger,
    AppSettings appSettings
) : IChatStreaming
{
    private readonly TimeSpan _defaultStreamItemTimeout = appSettings.StreamTimeout;

    public Task AddStreamingMessage(Guid conversationId, string text)
    {
        logger.LogInformation(
            "Adding streaming message for conversation {ConversationId}",
            conversationId
        );

        _ = StreamReplyAsync(conversationId, text)
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

        return Task.CompletedTask;
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

    private async Task StreamReplyAsync(Guid conversationId, string text)
    {
        var assistantReplyId = Guid.CreateVersion7();

        logger.LogInformation(
            "Processing message for conversation {ConversationId} {MessageId}",
            conversationId,
            assistantReplyId
        );

        var token = backplaneService.CancellationManager.GetCancellationToken(assistantReplyId);

        // Publish user message fragment
        var userFragment = new ClientMessageFragment(
            Guid.CreateVersion7(),
            ChatRole.User.Value,
            text,
            Guid.CreateVersion7(),
            true
        );
        await backplaneService.ConversationState.PublishFragmentAsync(conversationId, userFragment);

        // Publish initial assistant fragment
        var initialFragment = new ClientMessageFragment(
            assistantReplyId,
            ChatRole.Assistant.Value,
            "Generating reply...",
            Guid.CreateVersion7()
        );

        try
        {
            await backplaneService.ConversationState.PublishFragmentAsync(
                conversationId,
                initialFragment
            );

            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            tokenSource.CancelAfter(_defaultStreamItemTimeout);

            await foreach (
                var chunk in (
                    (
                        await orchestrationService.ProcessAgentsSequentiallyAsync(
                            text,
                            tokenSource.Token
                        )
                    ).WithCancellation(tokenSource.Token)
                )
            )
            {
                var finalFragment = new ClientMessageFragment(
                    assistantReplyId,
                    ChatRole.Assistant.Value,
                    chunk.Text,
                    Guid.CreateVersion7()
                );

                await backplaneService.ConversationState.PublishFragmentAsync(
                    conversationId,
                    finalFragment
                );
            }

            await backplaneService.ConversationState.CompleteAsync(
                conversationId,
                assistantReplyId
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

            var cancelledFragment = new ClientMessageFragment(
                assistantReplyId,
                ChatRole.Assistant.Value,
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
                ChatRole.Assistant.Value,
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
