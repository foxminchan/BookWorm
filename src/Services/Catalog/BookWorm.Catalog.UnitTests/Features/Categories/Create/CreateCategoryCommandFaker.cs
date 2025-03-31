using BookWorm.Catalog.Features.Categories.Create;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Create;

public sealed class CreateCategoryCommandFaker : Faker<CreateCategoryCommand>
{
    public CreateCategoryCommandFaker()
    {
        CustomInstantiator(f => new(f.Commerce.Categories(1)[0]));
    }

    public CreateCategoryCommand Generate()
    {
        return Generate(1)[0];
    }
}
