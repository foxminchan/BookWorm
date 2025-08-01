using BookWorm.Chassis.Repository;
using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Evaluators;
using BookWorm.Notification.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Notification.Infrastructure.Repositories;

public sealed class OutboxRepository(NotificationDbContext context) : IOutboxRepository
{
    private readonly NotificationDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task AddAsync(Outbox outbox, CancellationToken cancellationToken = default)
    {
        await _context.Outboxes.AddAsync(outbox, cancellationToken);
    }

    public async Task<IReadOnlyList<Outbox>> ListAsync(
        ISpecification<Outbox> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification.GetQuery(_context.Outboxes, spec).ToListAsync(cancellationToken);
    }

    public void BulkDelete(
        IEnumerable<Outbox> outboxes,
        CancellationToken cancellationToken = default
    )
    {
        _context.Outboxes.RemoveRange(outboxes);
    }
}
