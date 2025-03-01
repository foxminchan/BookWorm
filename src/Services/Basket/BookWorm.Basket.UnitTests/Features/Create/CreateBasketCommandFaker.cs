using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Create;

namespace BookWorm.Basket.UnitTests.Features.Create;

public sealed class CreateBasketCommandFaker : Faker<CreateBasketCommand>
{
    public CreateBasketCommandFaker()
    {
        CustomInstantiator(f =>
            new(
                [
                    .. f.Make(
                        f.Random.Int(1, 10),
                        () => new BasketItemRequest(f.Commerce.ProductName(), f.Random.Int(1, 10))
                    ),
                ]
            )
        );
    }

    public CreateBasketCommand Generate()
    {
        return Generate(1).First();
    }
}
