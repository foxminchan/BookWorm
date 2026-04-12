using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.Repository;

public interface IRepository<T>
    where T : IAggregateRoot
{
    /// <summary>
    ///     Gets the current unit of work used to coordinate transactional changes for this repository.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}
