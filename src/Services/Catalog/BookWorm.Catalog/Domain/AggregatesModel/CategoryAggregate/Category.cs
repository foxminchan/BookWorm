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

    public void UpdateName(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Category name must be provided.");
    }
}
