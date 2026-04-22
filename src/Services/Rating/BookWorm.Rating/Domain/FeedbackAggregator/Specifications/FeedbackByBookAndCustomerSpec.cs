using BookWorm.Chassis.Specification;
using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Rating.Domain.FeedbackAggregator.Specifications;

public sealed class FeedbackByBookAndCustomerSpec : Specification<Feedback>
{
    public FeedbackByBookAndCustomerSpec(Guid bookId, string? firstName, string? lastName)
    {
        Query.Where(f => f.BookId == bookId && f.FirstName == firstName && f.LastName == lastName);
    }
}
