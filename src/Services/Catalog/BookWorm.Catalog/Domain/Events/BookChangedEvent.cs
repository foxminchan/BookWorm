namespace BookWorm.Catalog.Domain.Events;

internal sealed class BookChangedEvent(string key) : DomainEvent
{
    public string Key { get; init; } = key;
}
