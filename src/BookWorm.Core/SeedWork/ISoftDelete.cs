namespace BookWorm.Core.SeedWork;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    void Delete();
}
