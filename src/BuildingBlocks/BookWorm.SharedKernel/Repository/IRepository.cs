using BookWorm.SharedKernel.SeedWork.Model;

namespace BookWorm.SharedKernel.Repository;

public interface IRepository<T>
    where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}
