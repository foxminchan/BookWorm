using BookWorm.SharedKernel.Specification;
using BookWorm.SharedKernel.Specification.Evaluators;

namespace BookWorm.Rating.Infrastructure.Repositories;

public sealed class FeedbackRepository(RatingDbContext context) : IFeedbackRepository
{
    private readonly RatingDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Feedback> AddAsync(
        Feedback feedback,
        CancellationToken cancellationToken = default
    )
    {
        var entry = await _context.Feedbacks.AddAsync(feedback, cancellationToken);
        return entry.Entity;
    }

    public async Task<Feedback?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var feedback = await _context
            .Feedbacks.AsNoTracking()
            .SingleOrDefaultAsync(f => f.Id == id, cancellationToken);

        return feedback;
    }

    public async Task<IReadOnlyList<Feedback>> ListAsync(
        ISpecification<Feedback> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(_context.Feedbacks.AsQueryable(), spec)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(
        ISpecification<Feedback> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(_context.Feedbacks.AsQueryable(), spec)
            .LongCountAsync(cancellationToken);
    }

    public void Delete(Feedback feedback)
    {
        _context.Feedbacks.Remove(feedback);
    }
}
