using BookWorm.Catalog.Features.Books.Create;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookCommandFaker : Faker<CreateBookCommand>
{
    public CreateBookCommandFaker()
    {
        CustomInstantiator(f =>
            new(
                f.Commerce.ProductName(),
                f.Lorem.Paragraph(),
                null,
                f.Finance.Amount(100),
                f.Finance.Amount(1, 99),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [Guid.CreateVersion7()]
            )
        );
    }

    public CreateBookCommand Generate()
    {
        return Generate(1)[0];
    }
}
