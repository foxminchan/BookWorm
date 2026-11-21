namespace BookWorm.Catalog.Domain.Events;

internal sealed class BookChangedEvent(BookId bookId) : DomainEvent
{
    public BookId BookId { get; init; } = bookId;
}
