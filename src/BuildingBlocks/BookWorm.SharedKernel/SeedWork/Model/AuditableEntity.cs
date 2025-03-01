namespace BookWorm.SharedKernel.SeedWork.Model;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public Guid Version { get; set; } = Guid.CreateVersion7();
}

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : struct, IEquatable<TId>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public Guid Version { get; set; } = Guid.CreateVersion7();
}
