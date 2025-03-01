using BookWorm.Catalog.Domain.Events;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace BookWorm.Catalog.Domain.EventHandlers;

public sealed class BookUpsertEmbeddingHandler(
    ILogger<BookUpsertEmbeddingHandler> logger,
    IAiService aiService,
    QdrantClient qdrantClient
) : INotificationHandler<BookCreatedEvent>, INotificationHandler<BookUpdatedEvent>
{
    private static readonly string _collection = nameof(Book).ToLowerInvariant();

    public async Task Handle(BookCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{EventName}] Handling event for book with id {BookId}",
                nameof(BookCreatedEvent),
                notification.Book.Id
            );
        }

        ArgumentException.ThrowIfNullOrEmpty(notification.Book.Name);
        ArgumentException.ThrowIfNullOrEmpty(notification.Book.Description);

        if (!await qdrantClient.CollectionExistsAsync(_collection, cancellationToken))
        {
            await qdrantClient.CreateCollectionAsync(
                _collection,
                new VectorParams { Size = 768, Distance = Distance.Cosine },
                sparseVectorsConfig: new(),
                cancellationToken: cancellationToken
            );
        }

        await ProcessEmbeddingAsync(
            notification.Book.Id,
            notification.Book.Name,
            notification.Book.Description,
            cancellationToken
        );
    }

    public async Task Handle(BookUpdatedEvent notification, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "[{EventName}] Handling event for book with id {BookId}",
                nameof(BookUpdatedEvent),
                notification.Book.Id
            );
        }

        ArgumentException.ThrowIfNullOrEmpty(notification.Book.Name);
        ArgumentException.ThrowIfNullOrEmpty(notification.Book.Description);

        await ProcessEmbeddingAsync(
            notification.Book.Id,
            notification.Book.Name,
            notification.Book.Description,
            cancellationToken
        );
    }

    private async Task ProcessEmbeddingAsync(
        Guid id,
        string name,
        string description,
        CancellationToken cancellationToken
    )
    {
        var embedding = await aiService.GetEmbeddingAsync(
            $"{name} {description}",
            cancellationToken
        );

        var pointStruct = new PointStruct
        {
            Id = id,
            Vectors = embedding.ToArray(),
            Payload = { ["Name"] = name, ["Description"] = description },
        };

        await qdrantClient.UpsertAsync(
            _collection,
            [pointStruct],
            cancellationToken: cancellationToken
        );
    }
}
