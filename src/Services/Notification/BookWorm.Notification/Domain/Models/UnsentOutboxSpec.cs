using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Notification.Domain.Models;

internal sealed class UnsentOutboxSpec : Specification<Outbox>
{
    public UnsentOutboxSpec()
    {
        Query.AsTracking().Where(x => !x.IsSent).OrderBy(x => x.SequenceNumber);
    }
}
