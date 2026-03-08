namespace BookWorm.Catalog.Domain.Events;

internal sealed class BookUpdatedEvent(Book book) : DomainEvent
{
    public Book Book { get; init; } = book;
}
