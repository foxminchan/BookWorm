using Ardalis.Specification.EntityFrameworkCore;

namespace BookWorm.Catalog.Infrastructure.Data;

public sealed class CatalogRepository<T>(CatalogContext dbContext)
    : RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot;
