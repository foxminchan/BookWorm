using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Constants.Other;

namespace BookWorm.Catalog.UnitTests.Fakers;

public sealed class PublisherFaker : Faker<Publisher>
{
    public PublisherFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f => new(f.Company.CompanyName()))
            .RuleFor(publisher => publisher.Id, _ => PublisherId.From(Guid.CreateVersion7()));
    }

    public Publisher[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
