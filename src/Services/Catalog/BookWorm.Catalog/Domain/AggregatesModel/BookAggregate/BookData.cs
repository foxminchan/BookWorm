namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[ExcludeFromCodeCoverage]
public sealed class BookData : List<Book>
{
    public BookData()
    {
        AddRange(
            [
                CreateBook("The Great Gatsby", 10.99m, 5.2m),
                CreateBook("To Kill a Mockingbird", 7.99m, 4.5m),
                CreateBook("1984", 8.99m, 4.8m),
                CreateBook("Pride and Prejudice", 6.99m, 4.2m),
                CreateBook("The Catcher in the Rye", 9.99m, 4.0m),
                CreateBook("The Hobbit", 12.99m, 6.5m),
                CreateBook("The Lord of the Rings", 20.99m, 10.0m),
                CreateBook("The Da Vinci Code", 14.99m, 7.0m),
                CreateBook("The Alchemist", 11.99m, 5.8m),
                CreateBook("The Hunger Games", 13.99m, 6.2m),
                CreateBook("The Fault in Our Stars", 9.99m, 4.5m),
                CreateBook("The Book Thief", 10.99m, 5.0m),
                CreateBook("The Kite Runner", 8.99m, 4.7m),
                CreateBook("The Fault in Our Stars", 9.99m, 4.5m),
                CreateBook("The Great Alone", 11.99m, 5.2m),
                CreateBook("The Night Circus", 13.99m, 6.5m),
                CreateBook("Circe", 14.99m, 7.0m),
                CreateBook("The Silent Patient", 15.99m, 7.5m),
                CreateBook("Anxious People", 17.99m, 8.5m),
                CreateBook("Atomic Habits", 18.99m, 9.0m),
            ]
        );
    }

    private static Book CreateBook(
        string name,
        decimal price,
        decimal? priceSale,
        string? description = null
    )
    {
        return new(name, description, null, price, priceSale, Guid.Empty, Guid.Empty, []);
    }
}
