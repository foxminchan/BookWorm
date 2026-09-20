using BookWorm.Constants.Other;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Ordering.UnitTests.Fakers;

public sealed class OrderFaker : Faker<Order>
{
    public OrderFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f =>
            new(
                BuyerId.From(f.Random.Guid()),
                f.Random.String2(1, 100),
                [.. new OrderItemFaker().Generate()]
            )
        );
    }

    public Order[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
