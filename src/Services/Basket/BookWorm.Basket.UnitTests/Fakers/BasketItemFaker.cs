using BookWorm.Basket.Domain;
using BookWorm.Constants;
using BookWorm.Constants.Other;

namespace BookWorm.Basket.UnitTests.Fakers;

public sealed class BasketItemFaker : Faker<BasketItem>
{
    public BasketItemFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f => new(f.Commerce.ProductName(), f.Random.Int(1, 10)));
    }

    public BasketItem[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
