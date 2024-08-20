namespace BookWorm.Rating.Repositories;

public interface IRatingRepository
{
    Task AddAsync(Feedback feedback, CancellationToken cancellationToken);
    Task DeleteAsync(FilterDefinition<Feedback> filter, CancellationToken cancellationToken);
    Task UpdateAsync(FilterDefinition<Feedback> filter, Feedback feedback, CancellationToken cancellationToken);

    Task<IEnumerable<Feedback>> ListAsync(FilterDefinition<Feedback> filter, int pageIndex, int pageSize,
        CancellationToken cancellationToken);

    Task<Feedback?> GetAsync(FilterDefinition<Feedback> filter, CancellationToken cancellationToken);
    Task<long> CountAsync(FilterDefinition<Feedback> filter, CancellationToken cancellationToken);
}
