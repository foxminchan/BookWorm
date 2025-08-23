namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

public sealed class Author() : Entity, IAggregateRoot
{
    private readonly List<BookAuthor> _bookAuthors = [];

    public Author(string name)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Author name must be provided.");
    }

    [DisallowNull]
    public string? Name { get; private set; }

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    /// <summary>
    ///     Updates the name of the author.
    /// </summary>
    /// <param name="name">The new name for the author. Cannot be null or whitespace.</param>
    /// <returns>The current Author instance for method chaining.</returns>
    /// <exception cref="CatalogDomainException">Thrown when the provided name is null or whitespace.</exception>
    public Author UpdateName(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Author name must be provided.");
        return this;
    }
}
