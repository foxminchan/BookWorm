using BookWorm.Chassis.Specification;

namespace BookWorm.Rating.Domain.FeedbackAggregator;

public interface IFeedbackRepository : IRepository<Feedback>
{
    Task<Feedback> AddAsync(Feedback feedback, CancellationToken cancellationToken = default);
    Task<Feedback?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Feedback>> ListAsync(
        ISpecification<Feedback> spec,
        CancellationToken cancellationToken = default
    );

    Task<long> CountAsync(
        ISpecification<Feedback> spec,
        CancellationToken cancellationToken = default
    );

    void Delete(Feedback feedback);
}
