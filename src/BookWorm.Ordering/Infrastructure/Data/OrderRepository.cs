using Ardalis.Specification.EntityFrameworkCore;
using BookWorm.Core.SeedWork;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Ordering.Infrastructure.Data;

public sealed class OrderRepository<T>(OrderContext dbContext)
    : RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot;
