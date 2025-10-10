using BookWorm.Catalog.Domain.Events;
using BookWorm.Catalog.Extensions;
using BookWorm.Chassis.AI.Ingestion;
using Mediator;

namespace BookWorm.Catalog.Domain.EventHandlers;

public sealed class BookUpsertEmbeddingHandler(
    ILogger<BookUpsertEmbeddingHandler> logger,
    IIngestionSource<Book> ingestion
) : INotificationHandler<BookCreatedEvent>, INotificationHandler<BookUpdatedEvent>
{
    public async ValueTask Handle(
        BookCreatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        BookApiTrace.LogBookCreated(logger, notification.Book.Id);
        await ingestion.IngestDataAsync(notification.Book, cancellationToken);
    }

    public async ValueTask Handle(
        BookUpdatedEvent notification,
        CancellationToken cancellationToken
    )
    {
        BookApiTrace.LogBookUpdated(logger, notification.Book.Id);
        await ingestion.IngestDataAsync(notification.Book, cancellationToken);
    }
}
