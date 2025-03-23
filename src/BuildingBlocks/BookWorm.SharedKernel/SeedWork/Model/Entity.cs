using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.SharedKernel.SeedWork.Model;

public abstract class Entity : HasDomainEventsBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}

public abstract class Entity<TId> : Entity
    where TId : IEquatable<TId>
{
    public new TId Id { get; set; } = default!;
}
