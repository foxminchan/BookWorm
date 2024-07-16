namespace BookWorm.Core.SeedWork;

public abstract class EntityBase : HasDomainEventsBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public Guid Version { get; set; }
}
