namespace BookWorm.Catalog.Domain.Events;

internal sealed class BookCreatedEvent(Book book) : DomainEvent
{
    public Book Book { get; init; } = book;
}
