using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Constants;
using BookWorm.Constants.Other;

namespace BookWorm.Catalog.UnitTests.Fakers;

public sealed class BookFaker : Faker<Book>
{
    public BookFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);

        // Create instances of related fakers
        var categoryFaker = new CategoryFaker();
        var publisherFaker = new PublisherFaker();
        var authorFaker = new AuthorFaker();

        // Generate related entities
        var categories = categoryFaker.Generate();
        var publishers = publisherFaker.Generate();
        var authors = authorFaker.Generate();

        CustomInstantiator(f =>
            new(
                f.Commerce.ProductName(),
                f.Lorem.Paragraph(),
                f.Random.Bool(0.8f) ? f.Image.PicsumUrl() : null,
                decimal.Parse(f.Commerce.Price(100)),
                f.Random.Bool(0.3f) ? decimal.Parse(f.Commerce.Price(1, 80)) : null,
                f.PickRandom(categories).Id,
                f.PickRandom(publishers).Id,
                [.. f.Random.ArrayElements(authors.Select(a => a.Id).ToArray(), f.Random.Int(1, 3))]
            )
        );
    }

    public Book[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
