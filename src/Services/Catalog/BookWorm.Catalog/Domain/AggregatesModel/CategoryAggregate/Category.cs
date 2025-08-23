namespace BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;

public sealed class Category() : Entity, IAggregateRoot
{
    public Category(string name)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Category name must be provided.");
    }

    [DisallowNull]
    public string? Name { get; private set; }

    /// <summary>
    ///     Updates the name of the category.
    /// </summary>
    /// <param name="name">The new name for the category. Cannot be null or whitespace.</param>
    /// <returns>The current category instance with the updated name.</returns>
    /// <exception cref="CatalogDomainException">Thrown when the provided name is null or contains only whitespace.</exception>
    public Category UpdateName(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Category name must be provided.");
        return this;
    }
}
