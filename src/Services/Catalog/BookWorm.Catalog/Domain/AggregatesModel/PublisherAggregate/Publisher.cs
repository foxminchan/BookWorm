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

    /// <summary>
    ///     Updates the name of the publisher.
    /// </summary>
    /// <param name="name">The new name for the publisher. Cannot be null or whitespace.</param>
    /// <returns>The current Publisher instance for method chaining.</returns>
    /// <exception cref="CatalogDomainException">Thrown when the provided name is null or whitespace.</exception>
    public Publisher UpdateName(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Publisher name must be provided.");
        return this;
    }
}
