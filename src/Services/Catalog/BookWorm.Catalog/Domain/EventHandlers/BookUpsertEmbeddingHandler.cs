using BookWorm.Catalog.Domain.Events;
using BookWorm.Catalog.Infrastructure.GenAi.Ingestion;

namespace BookWorm.Catalog.Domain.EventHandlers;

public sealed class BookUpsertEmbeddingHandler(
    ILogger<BookUpsertEmbeddingHandler> logger,
    IIngestionSource<Book> ingestion
) : INotificationHandler<BookCreatedEvent>, INotificationHandler<BookUpdatedEvent>
{
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

        await ingestion.IngestDataAsync(notification.Book, cancellationToken);
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

        await ingestion.IngestDataAsync(notification.Book, cancellationToken);
    }
}
