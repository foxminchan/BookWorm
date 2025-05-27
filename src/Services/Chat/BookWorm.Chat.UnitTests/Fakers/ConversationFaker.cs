using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Constants.Other;

namespace BookWorm.Chat.UnitTests.Fakers;

public sealed class ConversationFaker : Faker<Conversation>
{
    public ConversationFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);

        CustomInstantiator(f =>
            new(f.Lorem.Sentence(), f.Random.Bool(0.5f) ? f.Random.Guid() : null)
        );
    }

    public Conversation[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
