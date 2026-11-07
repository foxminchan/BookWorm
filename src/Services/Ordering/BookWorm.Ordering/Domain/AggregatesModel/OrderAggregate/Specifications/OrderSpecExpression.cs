using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

public static class OrderSpecExpression
{
    extension(ISpecificationBuilder<Order> builder)
    {
        public void ApplyPaging(int pageIndex, int pageSize)
        {
            builder.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
