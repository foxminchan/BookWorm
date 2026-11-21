using BookWorm.Catalog.Domain.Events;

namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

public sealed class Author() : Entity<AuthorId>, IAggregateRoot
{
    private readonly List<BookAuthor> _bookAuthors = [];

    public Author(string name)
        : this()
    {
        Name = name;
        RegisterDomainEvent(new AuthorCreatedEvent());
    }

    [DisallowNull]
    public string? Name
    {
        get;
        private set =>
            field = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new CatalogDomainException("Author name must be provided.");
    }

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    /// <summary>
    ///     Updates the name of the author.
    /// </summary>
    /// <param name="name">The new name for the author. Cannot be null or whitespace.</param>
    /// <returns>The current Author instance for method chaining.</returns>
    /// <exception cref="CatalogDomainException">Thrown when the provided name is null or whitespace.</exception>
    public Author UpdateName(string name)
    {
        Name = name;
        RegisterDomainEvent(new AuthorUpdatedEvent());
        return this;
    }

    /// <summary>
    ///     Marks the author for removal and registers a domain event.
    /// </summary>
    /// <returns>The current Author instance.</returns>
    public Author Remove()
    {
        RegisterDomainEvent(new AuthorDeletedEvent());
        return this;
    }
}
