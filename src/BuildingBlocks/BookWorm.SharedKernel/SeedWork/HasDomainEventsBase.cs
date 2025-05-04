using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookWorm.SharedKernel.SeedWork;

public abstract class HasDomainEventsBase
{
    private readonly List<DomainEvent> _domainEvents = [];

    [NotMapped]
    [JsonIgnore]
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RegisterDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
