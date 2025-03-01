using BookWorm.Basket.Domain;
using BookWorm.Constants;

namespace BookWorm.Basket.UnitTests.Fakers;

public sealed class CustomerBasketFaker : Faker<CustomerBasket>
{
    public CustomerBasketFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f =>
            new(f.Random.Uuid().ToString(), new BasketItemFaker().Generate().ToList())
        );
    }

    public CustomerBasket[] Generate()
    {
        return [.. Generate(Seeder.DefaultSeed)];
    }
}
