using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Constants.Other;

namespace BookWorm.Catalog.UnitTests.Fakers;

public sealed class AuthorFaker : Faker<Author>
{
    public AuthorFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f => new(f.Name.FullName()));
    }

    public Author[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
