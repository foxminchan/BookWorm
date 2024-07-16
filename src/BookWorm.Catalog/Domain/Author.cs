using Ardalis.GuardClauses;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Core.SeedWork;

namespace BookWorm.Catalog.Domain;

public sealed class Author : EntityBase, IAggregateRoot
{
    private readonly List<BookAuthor> _bookAuthors = [];

    private Author()
    {
        // EF Core
    }

    public Author(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }

    public string? Name { get; private set; }

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    public void UpdateName(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }
}
