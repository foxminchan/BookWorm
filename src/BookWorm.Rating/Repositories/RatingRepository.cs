namespace BookWorm.Rating.Repositories;

public sealed class RatingRepository(IMongoCollection<Feedback> collection) : IRatingRepository
{
    public async Task AddAsync(Feedback feedback, CancellationToken cancellationToken)
    {
        await collection.InsertOneAsync(feedback, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(FilterDefinition<Feedback> filter, CancellationToken cancellationToken)
    {
        await collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task UpdateAsync(FilterDefinition<Feedback> filter, Feedback feedback,
        CancellationToken cancellationToken)
    {
        await collection.ReplaceOneAsync(filter, feedback, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<Feedback>> ListAsync(FilterDefinition<Feedback> filter, int pageIndex, int pageSize,
        CancellationToken cancellationToken)
    {
        return await collection.Find(filter).Skip(pageIndex * pageSize).Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Feedback?> GetAsync(FilterDefinition<Feedback> filter, CancellationToken cancellationToken)
    {
        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<long> CountAsync(FilterDefinition<Feedback> filter, CancellationToken cancellationToken)
    {
        return await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }
}
