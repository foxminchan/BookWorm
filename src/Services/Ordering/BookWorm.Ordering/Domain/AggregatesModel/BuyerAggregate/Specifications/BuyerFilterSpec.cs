using BookWorm.Chassis.Specification.Builders;

namespace BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate.Specifications;

public sealed class BuyerFilterSpec : Specification<Buyer>
{
    public BuyerFilterSpec(int pageIndex, int pageSize)
    {
        Query.AsNoTracking().Skip((pageIndex - 1) * pageSize).Take(pageSize);
    }
}
