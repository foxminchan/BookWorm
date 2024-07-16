using System.ComponentModel.DataAnnotations.Schema;

namespace BookWorm.Core.SeedWork;

public abstract class HasDomainEventsBase
{
    private readonly List<EventBase> _domainEvents = [];

    [NotMapped] public IReadOnlyCollection<EventBase> DomainEvents => _domainEvents.AsReadOnly();

    public void RegisterDomainEvent(EventBase domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(EventBase domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
