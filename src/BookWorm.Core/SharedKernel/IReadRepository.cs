using BookWorm.Core.SeedWork;

namespace BookWorm.Core.SharedKernel;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot;
