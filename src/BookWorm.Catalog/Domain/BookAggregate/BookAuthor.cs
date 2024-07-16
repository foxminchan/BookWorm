using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Catalog.Domain.BookAggregate;

public class BookAuthor : EntityBase
{
    private BookAuthor()
    {
        // EF Core
    }

    public BookAuthor(Guid authorId) => AuthorId = Guard.Against.Default(authorId);

    public Guid AuthorId { get; private set; }
    public Author Author { get; private set; } = default!;
    public Book Book { get; private set; } = default!;
}
