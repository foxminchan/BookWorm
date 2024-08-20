namespace BookWorm.Catalog.Domain;

public sealed class Category : EntityBase, IAggregateRoot
{
    private Category()
    {
        // EF Core
    }

    public Category(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }

    public string? Name { get; private set; }

    public void UpdateName(string name)
    {
        Name = Guard.Against.NullOrEmpty(name);
    }
}
