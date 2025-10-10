namespace BookWorm.SharedKernel.SeedWork;

public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
}
