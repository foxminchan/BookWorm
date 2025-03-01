using BookWorm.Constants;
using BookWorm.Rating.Domain.FeedbackAggregator;

namespace BookWorm.Rating.UnitTests.Fakers;

public sealed class FeedbackFaker : Faker<Feedback>
{
    public FeedbackFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);
        CustomInstantiator(f =>
            new(
                f.Random.Guid(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Lorem.Sentence(),
                f.Random.Int(0, 5)
            )
        );
    }

    public Feedback[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
