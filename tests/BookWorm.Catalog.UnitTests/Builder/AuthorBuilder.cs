using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.UnitTests.Builder;

public static class AuthorBuilder
{
    private static List<Author> _authors = [];

    public static List<Author> WithDefaultValues()
    {
        _authors = 
        [
            new("Martin Fowler"),
            new("Eric Evans"),
            new("Robert C. Martin"),
            new("Kent Beck"),
            new("Don Box"),
            new("Joshua Bloch")
        ];

        return _authors;
    }
}
