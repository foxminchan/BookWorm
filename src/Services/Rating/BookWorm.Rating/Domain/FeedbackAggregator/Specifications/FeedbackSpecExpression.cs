using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Rating.Domain.FeedbackAggregator.Specifications;

public static class FeedbackSpecExpression
{
    public static ISpecificationBuilder<Feedback> ApplyOrdering(
        this ISpecificationBuilder<Feedback> builder,
        string? orderBy,
        bool isDescending
    )
    {
        return orderBy switch
        {
            nameof(Feedback.Rating) => isDescending
                ? builder.OrderByDescending(x => x.Rating)
                : builder.OrderBy(x => x.Rating),
            nameof(Feedback.CreatedAt) => isDescending
                ? builder.OrderByDescending(x => x.CreatedAt)
                : builder.OrderBy(x => x.CreatedAt),
            _ => isDescending
                ? builder.OrderByDescending(x => x.Rating)
                : builder.OrderBy(x => x.Rating),
        };
    }

    public static void ApplyPaging(
        this ISpecificationBuilder<Feedback> builder,
        int pageIndex,
        int pageSize
    )
    {
        builder.Skip((pageIndex - 1) * pageSize).Take(pageSize);
    }
}
