using System.Diagnostics.CodeAnalysis;

namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

[ExcludeFromCodeCoverage]
public sealed class BookData : List<Book>
{
    public BookData()
    {
        const string fakeDescription = "Description";

        AddRange(
            [
                new(
                    "The Great Gatsby",
                    fakeDescription,
                    null,
                    10.99m,
                    5.2m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "To Kill a Mockingbird",
                    fakeDescription,
                    null,
                    7.99m,
                    4.5m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new("1984", fakeDescription, null, 8.99m, 4.8m, Guid.Empty, Guid.Empty, []),
                new(
                    "Pride and Prejudice",
                    fakeDescription,
                    null,
                    6.99m,
                    4.2m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Catcher in the Rye",
                    fakeDescription,
                    null,
                    9.99m,
                    4.0m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new("The Hobbit", fakeDescription, null, 12.99m, 6.5m, Guid.Empty, Guid.Empty, []),
                new(
                    "The Lord of the Rings",
                    fakeDescription,
                    null,
                    20.99m,
                    10.0m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Da Vinci Code",
                    fakeDescription,
                    null,
                    14.99m,
                    7.0m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Alchemist",
                    fakeDescription,
                    null,
                    11.99m,
                    5.8m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Hunger Games",
                    fakeDescription,
                    null,
                    13.99m,
                    6.2m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new("The Fault in Our Stars", null, null, 9.99m, 4.5m, Guid.Empty, Guid.Empty, []),
                new(
                    "The Book Thief",
                    fakeDescription,
                    null,
                    10.99m,
                    5.0m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Kite Runner",
                    fakeDescription,
                    null,
                    8.99m,
                    4.7m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Fault in Our Stars",
                    fakeDescription,
                    null,
                    9.99m,
                    4.5m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Great Alone",
                    fakeDescription,
                    null,
                    11.99m,
                    5.2m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "The Night Circus",
                    fakeDescription,
                    null,
                    13.99m,
                    6.5m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new("Circe", fakeDescription, null, 14.99m, 7.0m, Guid.Empty, Guid.Empty, []),
                new(
                    "The Silent Patient",
                    fakeDescription,
                    null,
                    15.99m,
                    7.5m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "Anxious People",
                    fakeDescription,
                    null,
                    17.99m,
                    8.5m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
                new(
                    "Atomic Habits",
                    fakeDescription,
                    null,
                    18.99m,
                    9.0m,
                    Guid.Empty,
                    Guid.Empty,
                    []
                ),
            ]
        );
    }
}
