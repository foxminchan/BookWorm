using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;
using BookWorm.Constants.Core;

namespace BookWorm.Notification.Domain.Models;

internal sealed class UnsentOutboxSpec : Specification<Outbox>
{
    public UnsentOutboxSpec(int limit = Pagination.DefaultQueryLimit)
    {
        Query.AsTracking().Where(x => !x.IsSent).OrderBy(x => x.SequenceNumber).Take(limit);
    }
}
