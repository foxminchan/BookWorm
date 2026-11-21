using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Rating.Domain.FeedbackAggregator.Specifications;

public sealed class FeedbackFilterSpec : Specification<Feedback>
{
    public FeedbackFilterSpec(Guid bookId)
    {
        Query.AsNoTracking().Where(f => f.BookId == bookId);
    }

    public FeedbackFilterSpec(Guid bookId, string? orderBy, bool isDescending)
        : this(bookId)
    {
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            Query.ApplyOrdering(orderBy, isDescending);
        }
    }

    public FeedbackFilterSpec(
        Guid bookId,
        string? orderBy,
        bool isDescending,
        int pageIndex,
        int pageSize
    )
        : this(bookId, orderBy, isDescending)
    {
        Query.ApplyPaging(pageIndex, pageSize);
    }
}
