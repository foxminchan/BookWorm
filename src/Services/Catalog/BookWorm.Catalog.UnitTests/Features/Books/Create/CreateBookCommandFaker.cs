using BookWorm.Catalog.Features.Books.Create;

namespace BookWorm.Catalog.UnitTests.Features.Books.Create;

public sealed class CreateBookCommandFaker : Faker<CreateBookCommand>
{
    public CreateBookCommandFaker()
    {
        CustomInstantiator(f =>
            new()
            {
                Name = f.Commerce.ProductName(),
                Description = f.Lorem.Paragraph(),
                Price = f.Finance.Amount(100),
                PriceSale = f.Finance.Amount(1, 99),
                CategoryId = Guid.CreateVersion7(),
                PublisherId = Guid.CreateVersion7(),
                AuthorIds = [Guid.CreateVersion7()],
            }
        );
    }

    public CreateBookCommand Generate()
    {
        return Generate(1)[0];
    }
}
