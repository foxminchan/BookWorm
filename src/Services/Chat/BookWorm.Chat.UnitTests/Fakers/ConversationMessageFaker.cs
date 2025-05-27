using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Constants.Other;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Fakers;

public sealed class ConversationMessageFaker : Faker<ConversationMessage>
{
    public ConversationMessageFaker()
    {
        Randomizer.Seed = new(Seeder.DefaultSeed);

        CustomInstantiator(f =>
            new(
                f.Random.Bool(0.5f) ? f.Random.Guid() : null,
                f.Lorem.Sentence(),
                f.PickRandom(ChatRole.User.Value, ChatRole.Assistant.Value, ChatRole.System.Value),
                f.Random.Bool(0.3f) ? f.Random.Guid() : null
            )
        );
    }

    public ConversationMessage[] Generate()
    {
        return [.. Generate(Seeder.DefaultCount)];
    }
}
