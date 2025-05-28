﻿namespace BookWorm.SharedKernel.SeedWork;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public Guid Version { get; set; } = Guid.CreateVersion7();
}

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : IEquatable<TId>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public Guid Version { get; set; } = Guid.CreateVersion7();
}
