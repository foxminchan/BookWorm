using BookWorm.Rating.Features.Create;

namespace BookWorm.Rating.UnitTests.Features.Create;

public sealed class CreateFeedbackCommandFaker : Faker<CreateFeedbackCommand>
{
    public CreateFeedbackCommandFaker()
    {
        CustomInstantiator(f =>
            new(
                Guid.CreateVersion7(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Lorem.Sentence(),
                f.Random.Int(1, 5)
            )
        );
    }

    public CreateFeedbackCommand Generate()
    {
        return Generate(1).First();
    }
}
