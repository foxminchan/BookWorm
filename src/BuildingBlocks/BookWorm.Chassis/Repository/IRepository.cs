using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.Repository;

public interface IRepository<T>
    where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}
