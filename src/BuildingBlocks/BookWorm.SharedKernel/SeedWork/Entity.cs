namespace BookWorm.SharedKernel.SeedWork;

public abstract class Entity : HasDomainEventsBase
{
    public Guid Id { get; set; }
}

public abstract class Entity<TId> : Entity
    where TId : IEquatable<TId>
{
    public new TId Id { get; set; } = default!;
}
