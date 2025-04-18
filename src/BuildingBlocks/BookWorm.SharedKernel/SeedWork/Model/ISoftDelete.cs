namespace BookWorm.SharedKernel.SeedWork.Model;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    void Delete();
}
