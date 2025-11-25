using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Update;

namespace BookWorm.Basket.UnitTests.Features.Update;

public sealed class UpdateBasketCommandFaker : Faker<UpdateBasketCommand>
{
    public UpdateBasketCommandFaker()
    {
        CustomInstantiator(f =>
            new([
                .. f.Make(
                    f.Random.Int(1, 10),
                    () => new BasketItemRequest(f.Commerce.ProductName(), f.Random.Int(1, 10))
                ),
            ])
        );
    }

    public UpdateBasketCommand Generate()
    {
        return Generate(1)[0];
    }
}
