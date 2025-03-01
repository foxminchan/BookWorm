namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

public interface IPublisherRepository : IRepository<Publisher>
{
    Task<Publisher> AddAsync(Publisher publisher, CancellationToken cancellationToken);
    Task<IReadOnlyList<Publisher>> ListAsync(CancellationToken cancellationToken);
    Task<Publisher?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
