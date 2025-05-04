namespace BookWorm.SharedKernel.SeedWork;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    void Delete();
}
