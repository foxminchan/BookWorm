using BookWorm.Catalog.Domain.Events;

namespace BookWorm.Catalog.Extensions;

internal static partial class BookApiTrace
{
    [LoggerMessage(
        EventId = 0,
        EventName = nameof(BookCreatedEvent),
        Level = LogLevel.Debug,
        Message = "Book with Id {BookId} created"
    )]
    public static partial void LogBookCreated(ILogger logger, Guid bookId);

    [LoggerMessage(
        EventId = 1,
        EventName = nameof(BookUpdatedEvent),
        Level = LogLevel.Debug,
        Message = "Book with Id {BookId} updated"
    )]
    public static partial void LogBookUpdated(ILogger logger, Guid bookId);
}
