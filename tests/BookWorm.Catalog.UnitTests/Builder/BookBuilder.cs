using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.UnitTests.Builder;

public static class BookBuilder
{
    private static List<Book> _books = [];

    public static List<Book> WithDefaultValues()
    {
        _books =
        [
            new("Refactoring", 
                "Improving the Design of Existing Code",
                "http://example.com/image.jpg",
                96m, 10m, 
                Status.InStock, 
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]),
            new("Domain-Driven Design",
                "Tackling Complexity in the Heart of Software",
                null,
                75m, 8m,
                Status.InStock,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]),
            new("Clean Code",
                "A Handbook of Agile Software Craftsmanship",
                null,
                45m, 5m,
                Status.InStock,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]),
            new("Test Driven Development",
                "By Example",
                null,
                50m, 6m,
                Status.InStock,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]),
            new("Essential Scrum",
                "A Practical Guide to the Most Popular Agile Process",
                null,
                60m, 7m,
                Status.InStock,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]),
            new("Effective Java",
                "Programming Language Guide",
                null,
                55m, 6m,
                Status.InStock,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]),
            new("Design Patterns",
                "Elements of Reusable Object-Oriented Software",
                null,
                70m, 8m,
                Status.InStock,
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()])
        ];

        return _books;
    }
}
