using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Notification.Domain.Models;

public sealed class OutboxFilterSpec : Specification<Outbox>
{
    public OutboxFilterSpec()
    {
        Query.Where(x => x.IsSent).OrderBy(x => x.SequenceNumber);
    }
}
