using BookWorm.Core.SeedWork;

namespace BookWorm.Core.SharedKernel;

public interface IRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot;
