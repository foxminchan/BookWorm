using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;
using Pgvector;

namespace BookWorm.Catalog.Domain.BookAggregate;

public sealed class Book : EntityBase, IAggregateRoot, ISoftDelete
{
    private readonly List<BookAuthor> _bookAuthors = [];

    private Book()
    {
        // EF Core
    }

    public Book(
        string name,
        string? description,
        string? imageUrl,
        decimal price,
        decimal priceSale,
        Status status,
        Guid categoryId,
        Guid publisherId,
        List<Guid> authorIds)
    {
        Name = Guard.Against.NullOrEmpty(name);
        Description = Guard.Against.NullOrEmpty(description);
        ImageUrl = imageUrl;
        Price = new(price, priceSale);
        Status = status;
        CategoryId = categoryId;
        PublisherId = publisherId;
        _bookAuthors = [..authorIds.Select(authorId => new BookAuthor(authorId))];
    }

    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public Price? Price { get; private set; }
    public Status Status { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; }
    public Guid? PublisherId { get; private set; }
    public Publisher? Publisher { get; }
    [JsonIgnore] public Vector? Embedding { get; private set; }

    public IReadOnlyCollection<BookAuthor> BookAuthors => _bookAuthors.AsReadOnly();
    public bool IsDeleted { get; set; }

    public void Embed(Vector embedding)
    {
        Embedding = embedding;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void SetRating(double averageRating, int totalReviews)
    {
        AverageRating = Guard.Against.NegativeOrZero(averageRating);
        TotalReviews = Guard.Against.NegativeOrZero(totalReviews);
    }

    public void Update(
        string name,
        string? description,
        string? imageUrl,
        decimal price,
        decimal priceSale,
        Status status,
        Guid categoryId,
        Guid publisherId,
        List<Guid> authorIds)
    {
        Name = Guard.Against.NullOrEmpty(name);
        Description = Guard.Against.NullOrEmpty(description);
        ImageUrl = imageUrl;
        Price = new(price, priceSale);
        Status = status;
        CategoryId = categoryId;
        PublisherId = publisherId;
        _bookAuthors.Clear();
        _bookAuthors.AddRange(authorIds.Select(authorId => new BookAuthor(authorId)));
    }
}
