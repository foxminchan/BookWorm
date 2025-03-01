using BookWorm.SharedKernel.SeedWork.Model;

namespace BookWorm.SharedKernel.SeedWork;

public interface IRepository<T>
    where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}
