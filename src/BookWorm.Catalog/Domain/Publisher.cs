using BookWorm.Core.SeedWork;
using Ardalis.GuardClauses;

namespace BookWorm.Catalog.Domain;

public sealed class Publisher : EntityBase, IAggregateRoot
{
    private Publisher()
    {
        // EF Core
    }

    public Publisher(string name) => Name = Guard.Against.NullOrEmpty(name);

    public string? Name { get; private set; }

    public void UpdateName(string name) => Name = Guard.Against.NullOrEmpty(name);
}
