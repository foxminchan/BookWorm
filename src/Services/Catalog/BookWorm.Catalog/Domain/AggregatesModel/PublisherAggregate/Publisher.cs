namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

public sealed class Publisher() : Entity, IAggregateRoot
{
    public Publisher(string name)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Publisher name must be provided.");
    }

    [DisallowNull]
    public string? Name { get; private set; }

    public void UpdateName(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Publisher name must be provided.");
    }
}
