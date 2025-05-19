using BookWorm.Constants;
using BookWorm.Constants.Other;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

namespace BookWorm.Ordering.UnitTests.Fakers;

public sealed class OrderItemFaker : Faker<OrderItem>
{
    public OrderItemFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f =>
            new(f.Random.Guid(), f.Random.Int(1, 10), f.Random.Decimal(0, 100))
        );
    }

    public OrderItem[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
