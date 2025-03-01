using BookWorm.Ordering.Features.Buyers.Create;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Create;

public sealed class CreateBuyerCommandFaker : Faker<CreateBuyerCommand>
{
    public CreateBuyerCommandFaker()
    {
        CustomInstantiator(f =>
            new(f.Address.StreetAddress(), f.Address.City(), f.Address.State())
        );
    }

    public CreateBuyerCommand Generate()
    {
        return Generate(1).First();
    }
}
