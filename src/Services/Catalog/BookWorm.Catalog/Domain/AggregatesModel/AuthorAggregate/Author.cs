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

    public string? Name { get; private set; }

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();

    public void UpdateName(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Author name must be provided.");
    }
}
