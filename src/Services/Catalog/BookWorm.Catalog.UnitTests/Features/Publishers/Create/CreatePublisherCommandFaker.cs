using BookWorm.Catalog.Features.Publishers.Create;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Create;

public sealed class CreatePublisherCommandFaker : Faker<CreatePublisherCommand>
{
    public CreatePublisherCommandFaker()
    {
        CustomInstantiator(f => new(f.Company.CompanyName()));
    }

    public CreatePublisherCommand Generate()
    {
        return Generate(1).First();
    }
}
