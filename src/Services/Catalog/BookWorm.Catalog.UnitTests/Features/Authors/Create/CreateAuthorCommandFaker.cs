using BookWorm.Catalog.Features.Authors.Create;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Create;

public sealed class CreateAuthorCommandFaker : Faker<CreateAuthorCommand>
{
    public CreateAuthorCommandFaker()
    {
        CustomInstantiator(f => new(f.Person.FullName));
    }

    public CreateAuthorCommand Generate()
    {
        return Generate(1).First();
    }
}
