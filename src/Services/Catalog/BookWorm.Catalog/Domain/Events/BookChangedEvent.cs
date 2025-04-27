using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.Catalog.Domain.Events;

public sealed class BookChangedEvent(string key) : DomainEvent
{
    public string Key { get; init; } = key;
}
