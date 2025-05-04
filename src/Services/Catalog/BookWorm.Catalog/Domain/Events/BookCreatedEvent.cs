namespace BookWorm.Catalog.Domain.Events;

public sealed class BookCreatedEvent(Book book) : DomainEvent
{
    public Book Book { get; init; } = book;
}
