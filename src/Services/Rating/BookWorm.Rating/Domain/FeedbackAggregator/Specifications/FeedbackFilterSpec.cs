using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Rating.Domain.FeedbackAggregator.Specifications;

public sealed class FeedbackFilterSpec : Specification<Feedback>
{
    public FeedbackFilterSpec(Guid bookId, string? orderBy, bool isDescending)
    {
        Query.AsNoTracking();

        Query.Where(f => f.BookId == bookId);

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
