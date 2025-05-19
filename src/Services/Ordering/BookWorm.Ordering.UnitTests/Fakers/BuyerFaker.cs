using BookWorm.Constants;
using BookWorm.Constants.Other;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;

namespace BookWorm.Ordering.UnitTests.Fakers;

public sealed class BuyerFaker : Faker<Buyer>
{
    public BuyerFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f =>
            new(
                f.Random.Guid(),
                f.Name.FullName(),
                f.Address.StreetAddress(),
                f.Address.City(),
                f.Address.State()
            )
        );
    }

    public Buyer[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
