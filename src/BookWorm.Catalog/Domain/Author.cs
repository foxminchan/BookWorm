using BookWorm.Core.SeedWork;
using Ardalis.GuardClauses;
using BookWorm.Catalog.Domain.BookAggregate;

namespace BookWorm.Catalog.Domain;

public sealed class Author : EntityBase, IAggregateRoot
{
    private Author()
    {
        // EF Core
    }

    public Author(string name) => Name = Guard.Against.NullOrEmpty(name);

    public string? Name { get; private set; }

    private readonly List<BookAuthor> _bookAuthors = [];

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    public void UpdateName(string name) => Name = Guard.Against.NullOrEmpty(name);
}
