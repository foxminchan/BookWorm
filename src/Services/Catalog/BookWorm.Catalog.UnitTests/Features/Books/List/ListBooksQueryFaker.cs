using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Features.Books.List;

namespace BookWorm.Catalog.UnitTests.Features.Books.List;

public sealed class ListBooksQueryFaker : Faker<ListBooksQuery>
{
    public ListBooksQueryFaker()
    {
        CustomInstantiator(f =>
            new(
                f.Random.Int(1, 10),
                f.Random.Int(5, 50),
                f.Random.ArrayElement([
                    nameof(Book.Name),
                    nameof(Book.Price),
                    nameof(Book.CreatedAt),
                ]),
                f.Random.Bool(),
                f.Random.Bool() ? f.Lorem.Word() : null,
                f.Random.Bool() ? f.Random.Decimal(1, 50) : null,
                f.Random.Bool() ? f.Random.Decimal(51, 200) : null,
                f.Random.Bool() ? [.. f.Make(f.Random.Int(1, 5), Guid.CreateVersion7)] : null,
                f.Random.Bool() ? [.. f.Make(f.Random.Int(1, 3), Guid.CreateVersion7)] : null,
                f.Random.Bool() ? [.. f.Make(f.Random.Int(1, 4), Guid.CreateVersion7)] : null
            )
        );
    }

    public ListBooksQuery Generate()
    {
        return Generate(1)[0];
    }
}
