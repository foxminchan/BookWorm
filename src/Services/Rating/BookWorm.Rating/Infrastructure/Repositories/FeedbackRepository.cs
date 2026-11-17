using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Evaluators;

namespace BookWorm.Rating.Infrastructure.Repositories;

internal sealed class FeedbackRepository(RatingDbContext context) : IFeedbackRepository
{
    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => context;

    public async Task<Feedback> AddAsync(
        Feedback feedback,
        CancellationToken cancellationToken = default
    )
    {
        var entry = await context.Feedbacks.AddAsync(feedback, cancellationToken);
        return entry.Entity;
    }

    public async Task<Feedback?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .Feedbacks.AsTracking()
            .FirstOrDefaultAsync(f => f.Id == FeedbackId.From(id), cancellationToken);
    }

    public async Task<IReadOnlyList<Feedback>> ListAsync(
        ISpecification<Feedback> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(context.Feedbacks.AsQueryable(), spec)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(
        ISpecification<Feedback> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(context.Feedbacks.AsQueryable(), spec)
            .LongCountAsync(cancellationToken);
    }

    public void Delete(Feedback feedback)
    {
        context.Feedbacks.Remove(feedback);
    }
}
