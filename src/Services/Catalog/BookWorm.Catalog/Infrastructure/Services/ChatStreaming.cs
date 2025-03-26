using System.ComponentModel;
using System.Runtime.CompilerServices;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.Infrastructure.Services;

public sealed class ChatStreaming : IChatStreaming
{
    private readonly ICancellationManager _cancellationManager;
    private readonly IChatClient _chatClient;
    private readonly ChatOptions _chatOptions;
    private readonly IConversationState _conversationState;

    private readonly TimeSpan _defaultStreamItemTimeout = TimeSpan.FromMinutes(1);
    private readonly ILogger<ChatStreaming> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ChatStreaming(
        IChatClient chatClient,
        ILogger<ChatStreaming> logger,
        IConversationState conversationState,
        ICancellationManager cancellationManager,
        IServiceProvider serviceProvider
    )
    {
        _chatClient = chatClient;
        _logger = logger;
        _conversationState = conversationState;
        _cancellationManager = cancellationManager;
        _serviceProvider = serviceProvider;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "ChatModel: {model}",
                chatClient.GetService<ChatClientMetadata>()?.ModelId
            );
        }

        _chatOptions = new() { Tools = [AIFunctionFactory.Create(SearchCatalogAsync)] };

        Messages =
        [
            new(
                ChatRole.System,
                """
                You are an AI customer service assistant for BookWorm bookstore. You help customers find books and answer questions about our catalog.
                You ONLY respond to topics related to BookWorm.
                BookWorm is a book store that sells and provides information about books.
                Be concise and only provide detailed responses when necessary.
                If someone asks about anything other than BookWorm, its catalog, or their account, 
                politely refuse to answer and ask if there's a book-related topic you can assist with instead.
                """
            ),
            new(ChatRole.Assistant, "Hi! I'm the BookWorm assistant. How can I help you today?"),
        ];
    }

    private IList<ChatMessage> Messages { get; }

    /// <summary>
    ///     Adds a new user message to the conversation and starts streaming the AI response.
    /// </summary>
    /// <param name="text">The text message from the user.</param>
    /// <returns>A unique identifier for the conversation.</returns>
    public async Task<Guid> AddStreamingMessage(string text)
    {
        var conversationId = Guid.CreateVersion7();

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
                        .GetStreamingResponseAsync(Messages, _chatOptions)
                        .WithCancellation(tokenSource.Token)
                )
                {
                    tokenSource.CancelAfter(_defaultStreamItemTimeout);

                    if (chatResponseUpdate.Text is null)
                    {
                        continue;
                    }

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

    [Description("Searches the BookWorm catalog for a provided book description")]
    private async Task<string> SearchCatalogAsync(
        [Description("The product description for which to search")] string description
    )
    {
        var repository = _serviceProvider.GetRequiredService<IBookRepository>();

        var mapper = _serviceProvider.GetRequiredService<IMapper<Book, BookDto>>();

        var semanticSearch = _serviceProvider.GetRequiredService<IBookSemanticSearch>();

        var ids = await semanticSearch.FindBooksAsync(description);

        var books = await repository.ListAsync(new BookFilterSpec(ids));

        var results = mapper.MapToDtos(books);

        return JsonSerializer.Serialize(results);
    }
}
