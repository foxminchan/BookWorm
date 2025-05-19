using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Constants;
using BookWorm.Constants.Other;

namespace BookWorm.Catalog.UnitTests.Fakers;

public sealed class CategoryFaker : Faker<Category>
{
    public CategoryFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f => new(f.Commerce.Categories(1)[0]));
    }

    public Category[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
