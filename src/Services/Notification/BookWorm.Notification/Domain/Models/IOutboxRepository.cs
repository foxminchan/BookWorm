using BookWorm.Chassis.Repository;
using BookWorm.Chassis.Specification;

namespace BookWorm.Notification.Domain.Models;

public interface IOutboxRepository : IRepository<Outbox>
{
    Task AddAsync(Outbox outbox, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Outbox>> ListAsync(
        ISpecification<Outbox> spec,
        CancellationToken cancellationToken = default
    );

    void BulkDelete(IEnumerable<Outbox> outboxes, CancellationToken cancellationToken = default);
}
