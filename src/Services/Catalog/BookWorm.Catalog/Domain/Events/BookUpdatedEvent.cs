namespace BookWorm.Catalog.Domain.Events;

public sealed class BookUpdatedEvent(Book book) : DomainEvent
{
    public Book Book { get; init; } = book;
}
