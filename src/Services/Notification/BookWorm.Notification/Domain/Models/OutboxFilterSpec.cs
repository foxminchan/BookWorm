using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;
using BookWorm.Constants.Core;

namespace BookWorm.Notification.Domain.Models;

internal sealed class OutboxFilterSpec : Specification<Outbox>
{
    public OutboxFilterSpec(int limit = Pagination.DefaultQueryLimit)
    {
        Query.Where(x => x.IsSent).OrderBy(x => x.SequenceNumber).Take(limit);
    }
}
