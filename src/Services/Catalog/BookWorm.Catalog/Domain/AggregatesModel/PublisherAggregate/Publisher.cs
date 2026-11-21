using BookWorm.Catalog.Domain.Events;

namespace BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;

public sealed class Publisher() : Entity<PublisherId>, IAggregateRoot
{
    public Publisher(string name)
        : this()
    {
        Name = name;
        RegisterDomainEvent(new PublisherCreatedEvent());
    }

    [DisallowNull]
    public string? Name
    {
        get;
        private set =>
            field = !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new CatalogDomainException("Publisher name must be provided.");
    }

    /// <summary>
    ///     Updates the name of the publisher.
    /// </summary>
    /// <param name="name">The new name for the publisher. Cannot be null or whitespace.</param>
    /// <returns>The current Publisher instance for method chaining.</returns>
    /// <exception cref="CatalogDomainException">Thrown when the provided name is null or whitespace.</exception>
    public Publisher UpdateName(string name)
    {
        Name = name;
        RegisterDomainEvent(new PublisherUpdatedEvent());
        return this;
    }

    /// <summary>
    ///     Marks the publisher for removal and registers a domain event.
    /// </summary>
    /// <returns>The current Publisher instance.</returns>
    public Publisher Remove()
    {
        RegisterDomainEvent(new PublisherDeletedEvent());
        return this;
    }
}
